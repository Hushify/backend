using Amazon.CloudFront;
using Amazon.S3;
using Amazon.S3.Model;
using Hushify.Api.Features.Drive.Entities;
using Hushify.Api.Features.Drive.Services;
using Hushify.Api.Features.Identity.Entities;
using Hushify.Api.Features.Identity.Services;
using Hushify.Api.Options;
using Hushify.Api.Persistence;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Hushify.Api.Features.Drive.Endpoints;

public static class List
{
    public static IEndpointRouteBuilder MapListEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapGet("/list", ListHandler);

        routes.MapGet("/stats", async (IDriveService driveService, CancellationToken ct) =>
        {
            var stats = await driveService.GetDriveStatsAsync(ct);
            return TypedResults.Ok(new StorageStatsResponse(stats.Total, stats.Used));
        });

        routes.MapGet("/test", (CryptoKeys keys, IOptions<ConfigOptions> options, [FromQuery] string resource) =>
        {
            var key = new StringReader(keys.PrivateSecurityKey.Rsa.ExportRSAPrivateKeyPem());
            return AmazonCloudFrontUrlSigner.GetCannedSignedURL(AmazonCloudFrontUrlSigner.Protocol.https,
                options.Value.AWS.ServiceUrl, key, resource, options.Value.AWS.KeyId, DateTime.UtcNow.AddHours(12));
        }).AllowAnonymous();

        routes.MapGet("/test2", (IOptions<ConfigOptions> options, [FromQuery] string resource,
                [FromQuery] HttpVerb type,
                IAmazonS3 client) =>
            Task.FromResult(client.GetPreSignedURL(new GetPreSignedUrlRequest
            {
                BucketName = options.Value.AWS.BucketName,
                Protocol = Protocol.HTTPS,
                Key = resource,
                Expires = DateTime.UtcNow.AddHours(12),
                Verb = type
            }))).AllowAnonymous();

        return routes;
    }

    private static async Task<Results<Ok<ListResponse>, ValidationProblem>> ListHandler(
        Guid? folderId, IDriveService driveService,
        WorkspaceDbContext dbContext, CancellationToken ct) =>
        TypedResults.Ok(await driveService.ListAsync(folderId, ct));
}

public sealed record ListResponse(Guid WorkspaceFolderId, Guid CurrentFolderId, IEnumerable<BreadcrumbVM> Breadcrumbs,
    IEnumerable<FileNodeVM> Files, IEnumerable<FolderNodeVM> Folders);

public sealed record BreadcrumbVM(Guid Id, MetadataBundle? MetadataBundle, SecretKeyBundle FolderKey);

public sealed record FileNodeVM(Guid Id, MetadataBundle? MetadataBundle, long EncryptedSize, SecretKeyBundle FileKey,
    string FileUrl);

public sealed record FolderNodeVM(Guid Id, MetadataBundle? MetadataBundle, SecretKeyBundle FolderKey);

public sealed record StorageStatsResponse(long Total, long Used);