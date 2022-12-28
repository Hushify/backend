using Hushify.Api.Constants;
using Hushify.Api.Extensions;

namespace Hushify.Api.Features.Drive.Endpoints;

public static class DriveEndpoints
{
    public static RouteGroupBuilder MapDriveEndpoints(this IEndpointRouteBuilder routes)
    {
        var driveRoutes = routes.MapGroup("/drive");
        driveRoutes.WithTags("Drive");
        driveRoutes.RequireAuthorization().AddOpenApiSecurityRequirement();
        driveRoutes.RequireRateLimiting(AppConstants.IpRateLimit);

        driveRoutes.MapListEndpoints();
        driveRoutes.MapCreateEndpoints();

        return driveRoutes;
    }
}