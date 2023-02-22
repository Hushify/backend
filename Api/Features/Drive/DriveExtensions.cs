using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Hushify.Api.Constants;
using Hushify.Api.Extensions;
using Hushify.Api.Features.Drive.Endpoints;
using Hushify.Api.Features.Drive.Services;
using Hushify.Api.Options;
using Microsoft.Extensions.Options;

namespace Hushify.Api.Features.Drive;

public static class DriveExtensions
{
    public static WebApplicationBuilder AddDrive(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IAmazonS3, AmazonS3Client>(provider =>
        {
            AWSConfigsS3.UseSignatureVersion4 = true;
            var options = provider.GetRequiredService<IOptionsSnapshot<ConfigOptions>>().Value;
            var credentials = new BasicAWSCredentials(options.AWS.AccessKey, options.AWS.SecretKey);
            var s3Config = new AmazonS3Config
            {
                // ServiceURL = options.AWS.ServiceUrl,
                RegionEndpoint = RegionEndpoint.GetBySystemName("eu-west-2")
            };
            return new AmazonS3Client(credentials, s3Config);
        });

        builder.Services.AddScoped<IDriveService, DriveS3Service>();

        return builder;
    }

    public static RouteGroupBuilder MapDriveEndpoints(this IEndpointRouteBuilder routes)
    {
        var driveRoutes = routes.MapGroup("/drive");
        driveRoutes.WithTags("Drive");
        driveRoutes.RequireAuthorization().AddOpenApiSecurityRequirement();
        driveRoutes.RequireRateLimiting(AppConstants.IpRateLimit);

        driveRoutes.MapListEndpoints();
        driveRoutes.MapCreateEndpoints();
        driveRoutes.MapDeleteEndpoints();

        return driveRoutes;
    }
}