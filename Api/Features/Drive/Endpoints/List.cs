using Hushify.Api.Features.Drive.Services;
using Hushify.Api.Persistence;
using Hushify.Api.Persistence.Entities;
using Hushify.Api.Persistence.Entities.Drive;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Hushify.Api.Features.Drive.Endpoints;

public static class List
{
    public static IEndpointRouteBuilder MapListEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapGet("/list", ListHandler);
        return routes;
    }

    private static async Task<Results<Ok<ListResponse>, ValidationProblem>> ListHandler(
        [FromQuery] Guid? folderId, IDriveService driveService,
        WorkspaceDbContext dbContext, CancellationToken ct) =>
        TypedResults.Ok(await driveService.ListAsync(folderId, ct));
}

public sealed record ListResponse(Guid WorkspaceFolderId, Guid CurrentFolderId, IEnumerable<BreadcrumbVM> Breadcrumbs,
    IEnumerable<FileNodeVM> files,
    IEnumerable<FolderNodeVM> folders);

public sealed record BreadcrumbVM(Guid Id, MetadataBundle? MetadataBundle, SecretKeyBundle FolderKey);

public sealed record FileNodeVM(Guid Id, MetadataBundle? MetadataBundle, long EncryptedSize, SecretKeyBundle FileKey,
    string FileUrl);

public sealed record FolderNodeVM(Guid Id, MetadataBundle? MetadataBundle, SecretKeyBundle FolderKey);