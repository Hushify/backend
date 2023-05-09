using Hushify.Api.Exceptions;
using Hushify.Api.Extensions;
using Hushify.Api.Features.Identity.Entities;
using Hushify.Api.Features.Identity.Services;
using Hushify.Api.Options;
using Hushify.Api.Persistence;
using MassTransit;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Hushify.Api.Features.Identity.Endpoints;

public static class Refresh
{
    public static IEndpointRouteBuilder MapRefreshEndpoints(this RouteGroupBuilder routes)
    {
        routes.MapPost("/refresh", RefreshHandler);
        return routes;
    }

    private static async
        Task<Results<Ok<RefreshResponse>, ValidationProblem, UnauthorizedHttpResult>>
        RefreshHandler(
            UserManager<AppUser> userManager, IBus bus, IHttpContextAccessor ctxAccessor,
            ITokenGenerator tokenGenerator,
            IOptions<ConfigOptions> options, AppDbContext appDbContext, CancellationToken ct)
    {
        var ctx = ctxAccessor.HttpContext;
        if (ctx is null || !ctx.GetRefreshTokenHashFromCookie(out var tokenHash) ||
            string.IsNullOrWhiteSpace(tokenHash))
        {
            return TypedResults.Unauthorized();
        }

        var user = await appDbContext.Users.Include(x => x.RefreshTokens)
            .Where(x => x.RefreshTokens.Any(t => t.TokenHash == tokenHash)).FirstOrDefaultAsync(ct);

        if (user is null)
        {
            ctx.DeleteRefreshTokenCookie(options.Value.ApiUrl.Domain, out var _);
            throw new AppException("User was not found.", new[] { "User was not found." });
        }

        var userAgent = ctx.GetUserAgent();

        var refreshToken = user.RefreshTokens.Single(x => x.TokenHash == tokenHash);
        if (refreshToken.IsRevoked)
        {
            RefreshToken.RevokeDescendantRefreshTokens(refreshToken, user, userAgent,
                $"Attempted reuse of revoked ancestor token: {tokenHash}");
            appDbContext.Entry(user).State = EntityState.Modified;
            await userManager.UpdateAsync(user);
        }

        if (!refreshToken.IsActive)
        {
            ctx.DeleteRefreshTokenCookie(options.Value.ApiUrl.Domain, out _);
            throw new AppException("Invalid token.", new[] { "Invalid token." });
        }

        var newToken = string.Empty;
        var newTokenHash = refreshToken.TokenHash;
        if (refreshToken.Expires.Subtract(DateTimeOffset.UtcNow).Days < 3)
        {
            var (newRefreshToken, refToken) =
                RefreshToken.RotateRefreshToken(refreshToken, tokenGenerator, userAgent);
            user.RefreshTokens.Add(newRefreshToken);
            RefreshToken.RemoveOldRefreshTokens(user, options.Value.RefreshToken.TimeToLiveInDays);

            appDbContext.Entry(user).State = EntityState.Modified;
            await userManager.UpdateAsync(user);

            newToken = refToken;
            newTokenHash = newRefreshToken.TokenHash;
        }

        if (user.CryptoProperties is null)
        {
            throw new AppException("User has not been initialized yet.");
        }

        var (accessTokenNonce, encryptedAccessToken, serverPublicKey) =
            tokenGenerator.GenerateAccessToken(user.GetAccessTokenClaims(),
                user.CryptoProperties.AsymmetricKeyBundle.PublicKey);

        if (!string.Equals(newTokenHash, tokenHash))
        {
            ctx.SetRefreshTokenCookie(newToken, options.Value.RefreshToken.TimeToLiveInDays,
                options.Value.ApiUrl.Domain);
        }

        return TypedResults.Ok(new RefreshResponse(accessTokenNonce, encryptedAccessToken,
            serverPublicKey));
    }
}

public sealed record RefreshResponse(string AccessTokenNonce, string EncryptedAccessToken,
    string ServerPublicKey);