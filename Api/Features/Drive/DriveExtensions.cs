using Amazon;
using Amazon.Runtime;
using Amazon.S3;
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
            var options = provider.GetRequiredService<IOptionsSnapshot<ConfigOptions>>().Value;
            return new AmazonS3Client(new BasicAWSCredentials(options.AWS.AccessKey, options.AWS.SecretKey),
                RegionEndpoint.EUWest2);
        });

        builder.Services.AddScoped<IDriveService, DriveS3Service>();

        return builder;
    }
}