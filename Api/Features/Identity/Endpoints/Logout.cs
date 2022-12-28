using Hushify.Api.Exceptions;
using Hushify.Api.Features.Identity.Extensions;
using Hushify.Api.Options;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Options;

namespace Hushify.Api.Features.Identity.Endpoints;

public static class Logout
{
    public static IEndpointRouteBuilder MapLogoutEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapPost("/logout", LogoutHandler);
        return routes;
    }

    private static Ok LogoutHandler(IHttpContextAccessor ctxAccessor, IOptions<ConfigOptions> options)
    {
        var ctx = ctxAccessor.HttpContext ?? throw new AppException("HttpContext was null.");
        ctx.DeleteRefreshTokenCookie(options.Value.ApiUrl.Domain);
        return TypedResults.Ok();
    }
}