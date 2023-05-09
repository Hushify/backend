using Hushify.Api.Exceptions;
using Hushify.Api.Extensions;
using Hushify.Api.Features.Identity.Entities;
using Hushify.Api.Options;
using Hushify.Api.Persistence;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Hushify.Api.Features.Identity.Endpoints;

public static class Logout
{
    public static IEndpointRouteBuilder MapLogoutEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapPost("/logout", LogoutHandler);
        return routes;
    }

    private static async Task<Ok> LogoutHandler(IHttpContextAccessor ctxAccessor,
        IOptions<ConfigOptions> options,
        AppDbContext appDbContext, CancellationToken ct)
    {
        var ctx = ctxAccessor.HttpContext ?? throw new AppException("HttpContext was null.");
        if (!ctx.DeleteRefreshTokenCookie(options.Value.ApiUrl.Domain, out var tokenHash))
        {
            return TypedResults.Ok();
        }

        var user = await appDbContext.Users.Include(x => x.RefreshTokens)
            .Where(x => x.RefreshTokens.Any(t => t.TokenHash == tokenHash)).FirstOrDefaultAsync(ct);
        if (user is null)
        {
            return TypedResults.Ok();
        }

        var refreshToken = user.RefreshTokens.FirstOrDefault(x => x.TokenHash == tokenHash);
        if (refreshToken is null)
        {
            return TypedResults.Ok();
        }

        RefreshToken.RevokeRefreshToken(refreshToken, ctx.GetUserAgent(), "User logged out.");
        appDbContext.Entry(user).State = EntityState.Modified;
        await appDbContext.SaveChangesAsync(ct);

        return TypedResults.Ok();
    }
}