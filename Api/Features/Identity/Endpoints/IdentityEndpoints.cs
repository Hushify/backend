using Hushify.Api.Constants;

namespace Hushify.Api.Features.Identity.Endpoints;

public static class IdentityEndpoints
{
    public static RouteGroupBuilder MapIdentityEndpoints(this IEndpointRouteBuilder routes)
    {
        var identityRoutes = routes.MapGroup("/identity");
        identityRoutes.WithTags("Identity");
        identityRoutes.RequireRateLimiting(AppConstants.IpRateLimit);

        identityRoutes.MapRegisterEndpoints();
        identityRoutes.MapResendConfirmationEndpoints();
        identityRoutes.MapLoginEndpoints();
        identityRoutes.MapRefreshEndpoints();
        identityRoutes.MapForgotPasswordEndpoints();
        identityRoutes.MapLogoutEndpoints();

        return identityRoutes;
    }
}