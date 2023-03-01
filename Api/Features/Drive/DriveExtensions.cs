using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Hushify.Api.Extensions;
using Hushify.Api.Features.Drive.Endpoints;
using Hushify.Api.Features.Drive.Services;
using Hushify.Api.Options;

namespace Hushify.Api.Features.Drive;

public static class DriveExtensions
{
    public static void AddDrive(this WebApplicationBuilder builder, AWSOptions awsOptions)
    {
        builder.Services.AddScoped<IAmazonS3, AmazonS3Client>(provider =>
        {
            AWSConfigsS3.UseSignatureVersion4 = true;
            var credentials = new BasicAWSCredentials(awsOptions.AccessKey, awsOptions.SecretKey);
            var s3Config = new AmazonS3Config
            {
                // ServiceURL = options.AWS.ServiceUrl,
                RegionEndpoint = RegionEndpoint.GetBySystemName(awsOptions.Region)
            };
            return new AmazonS3Client(credentials, s3Config);
        });

        builder.Services.AddScoped<IDriveService, DriveS3Service>();
    }

    public static void MapDriveEndpoints(this IEndpointRouteBuilder routes)
    {
        var driveRoutes = routes.MapGroup("/drive");
        driveRoutes.WithTags("Drive");
        driveRoutes.RequireAuthorization().AddOpenApiSecurityRequirement();

        driveRoutes.MapListEndpoints();
        driveRoutes.MapCreateEndpoints();
        driveRoutes.MapDeleteEndpoints();
        driveRoutes.MapMetadataEndpoints();
        driveRoutes.MapMultipartUploadEndpoints();
    }
}