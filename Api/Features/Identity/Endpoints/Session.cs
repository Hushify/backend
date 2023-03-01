using Hushify.Api.Features.Identity.Entities;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using System.Globalization;

namespace Hushify.Api.Features.Identity.Endpoints;

public static class Session
{
    public static IEndpointRouteBuilder MapSessionEndpoints(this RouteGroupBuilder routes)
    {
        routes.MapGet("/sessions", GetSessionsHandler).RequireAuthorization().DisableRateLimiting();
        // routes.MapDelete("/sessions/{id:guid}", DeleteSessionHandler);
        return routes;
    }

    private static async Task<Results<Ok<GetSessionsResponse>, ValidationProblem>> GetSessionsHandler(
        UserManager<AppUser> manager, IHttpContextAccessor ctxAccessor, CancellationToken ct)
    {
        var user = await manager.GetUserAsync(ctxAccessor.HttpContext!.User);
        if (user is null)
        {
            return TypedResults.Ok(new GetSessionsResponse(Array.Empty<SessionDTO>()));
        }

        var tokens = user.RefreshTokens.Where(r => r.IsActive).Select(r =>
            new SessionDTO(r.Id, r.Created.UtcDateTime.ToString(CultureInfo.InvariantCulture),
                r.CreatedByUserAgent ?? string.Empty)).ToArray();

        return TypedResults.Ok(new GetSessionsResponse(tokens));
    }
}

public sealed record SessionDTO(string id, string created, string userAgent);

public sealed record GetSessionsResponse(SessionDTO[] Sessions);