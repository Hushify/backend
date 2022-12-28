using Hushify.Api.Exceptions;
using Hushify.Api.Extensions;
using Hushify.Api.Features.Identity.Extensions;
using Hushify.Api.Features.Identity.Services;
using Hushify.Api.Options;
using Hushify.Api.Persistence;
using Hushify.Api.Persistence.Entities;
using MassTransit;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Hushify.Api.Features.Identity.Endpoints;

public static class Refresh
{
    public static IEndpointRouteBuilder MapRefreshEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapPost("/refresh", RefreshHandler);
        return routes;
    }

    private static async Task<Results<Ok<RefreshResponse>, ValidationProblem, UnauthorizedHttpResult>> RefreshHandler(
        UserManager<AppUser> userManager, IBus bus, IHttpContextAccessor ctxAccessor, ITokenGenerator tokenGenerator,
        IOptions<ConfigOptions> options, AppDbContext appDbContext, CancellationToken ct)
    {
        var ctx = ctxAccessor.HttpContext;
        if (ctx is null || !ctx.GetRefreshTokenCookie(out var token) || string.IsNullOrWhiteSpace(token))
        {
            return TypedResults.Unauthorized();
        }

        var user = await appDbContext.Users.Include(x => x.RefreshTokens)
            .Where(x => x.RefreshTokens.Any(t => t.Token == token)).FirstOrDefaultAsync(ct);

        if (user is null)
        {
            ctx.DeleteRefreshTokenCookie(options.Value.ApiUrl.Domain);
            throw new AppException("User was not found.", new[] { "User was not found." });
        }

        var userAgent = ctx.GetUserAgent();

        var refreshToken = user.RefreshTokens.Single(x => x.Token == token);
        if (refreshToken.IsRevoked)
        {
            RevokeDescendantRefreshTokens(refreshToken, user, userAgent,
                $"Attempted reuse of revoked ancestor token: {token}");
            appDbContext.Entry(user).State = EntityState.Modified;
            await userManager.UpdateAsync(user);
        }

        if (!refreshToken.IsActive)
        {
            ctx.DeleteRefreshTokenCookie(options.Value.ApiUrl.Domain);
            throw new AppException("Invalid token.", new[] { "Invalid token." });
        }

        var newToken = refreshToken.Token;
        if (refreshToken.Expires.Subtract(DateTimeOffset.UtcNow).Days < 3)
        {
            var newRefreshToken = RotateRefreshToken(refreshToken, tokenGenerator, userAgent);
            user.RefreshTokens.Add(newRefreshToken);
            RemoveOldRefreshTokens(user, options.Value.RefreshToken.TimeToLiveInDays);

            appDbContext.Entry(user).State = EntityState.Modified;
            await userManager.UpdateAsync(user);

            newToken = newRefreshToken.Token;
        }

        if (user.AsymmetricEncKeyBundle is null)
        {
            throw new AppException("User has not been initialized yet.");
        }

        var (accessTokenNonce, encAccessToken, serverPublicKey) =
            tokenGenerator.GenerateAccessToken(user.GetAccessTokenClaims(), user.AsymmetricEncKeyBundle.PublicKey);

        var updateRefreshToken = !string.Equals(newToken, token);
        if (updateRefreshToken)
        {
            ctx.SetRefreshTokenCookie(newToken, options.Value.RefreshToken.TimeToLiveInDays,
                options.Value.ApiUrl.Domain);
        }

        return TypedResults.Ok(new RefreshResponse(accessTokenNonce, encAccessToken, serverPublicKey));
    }

    private static void RemoveOldRefreshTokens(AppUser user, int ttlInDays) =>
        user.RefreshTokens.RemoveAll(x =>
            !x.IsActive && x.Created.AddDays(ttlInDays) <= DateTime.UtcNow);

    private static void RevokeRefreshToken(RefreshToken token, string ipAddress, string userAgent,
        string? reason = null, string? replacedByTokenId = null)
    {
        token.Revoked = DateTime.UtcNow;
        token.RevokedByUserAgent = userAgent;
        token.ReasonRevoked = reason;
        token.ReplacedByTokenId = replacedByTokenId;
    }

    private static void RevokeDescendantRefreshTokens(RefreshToken refreshToken, AppUser user,
        string userAgent, string reason)
    {
        if (refreshToken.ReplacedByToken is null)
        {
            return;
        }

        var childToken = user.RefreshTokens.SingleOrDefault(x => x.Token == refreshToken.ReplacedByToken.Token);
        if (childToken!.IsActive)
        {
            RevokeRefreshToken(childToken, userAgent, reason);
        }
        else
        {
            RevokeDescendantRefreshTokens(childToken, user, userAgent, reason);
        }
    }

    private static RefreshToken RotateRefreshToken(RefreshToken refreshToken, ITokenGenerator tokenGenerator,
        string userAgent)
    {
        var newRefreshToken = tokenGenerator.GenerateRefreshToken(userAgent);
        RevokeRefreshToken(refreshToken, userAgent, "Replaced by new token", newRefreshToken.Id);
        return newRefreshToken;
    }
}

public sealed record RefreshResponse(string AccessTokenNonce, string EncAccessToken, string ServerPublicKey);