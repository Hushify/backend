using FluentValidation;
using Hushify.Api.Exceptions;
using Hushify.Api.Features.Drive.Entities;
using Hushify.Api.Features.Identity.Entities;
using Hushify.Api.Filters;
using Hushify.Api.Persistence;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Hushify.Api.Features.Drive.Endpoints;

public static class Move
{
    public static IEndpointRouteBuilder MapMoveEndpoints(this RouteGroupBuilder routes)
    {
        routes.WithParameterValidation(typeof(MoveRequest));

        routes.MapPost("/move-nodes", MoveFolderHandler);

        return routes;
    }

    private static async Task<Results<Ok, ValidationProblem>> MoveFolderHandler(
        MoveRequest req, WorkspaceDbContext ctx, CancellationToken ct)
    {
        var targetFolder =
            await ctx.Folders.FirstOrDefaultAsync(f => f.Id == req.TargetFolderId, ct);
        if (targetFolder is null)
        {
            throw new AppException("Invalid target folder.");
        }

        var folders =
            ctx.Folders.Where(f => req.Folders.Select(reqFolder => reqFolder.Id).Contains(f.Id));
        foreach (var folder in folders)
        {
            if (folder.Id == targetFolder.Id)
            {
                continue;
            }

            var reqFolder = req.Folders.First(r => r.Id == folder.Id);

            folder.ParentFolderId = targetFolder.Id;
            folder.MetadataBundle = reqFolder.MetadataBundle;
            folder.KeyBundle = reqFolder.KeyBundle;
            folder.MaterializedPath = $"{targetFolder.MaterializedPath}{folder.Id}/";
        }

        var files = await ctx.Files.Where(f =>
            req.Files.Select(reqFile => reqFile.Id).Contains(f.Id)
        ).ToListAsync(ct);
        var previousVersions = await ctx.Files.Where(f =>
            req.Files.Select(reqFile => reqFile.PreviousVersionId).Contains(f.Id)
        ).ToListAsync(ct);

        foreach (var file in files)
        {
            var reqFile = req.Files.First(r => r.Id == file.Id);
            var previousVersion =
                previousVersions.FirstOrDefault(f => f.Id == reqFile.PreviousVersionId);
            if (previousVersion is not null)
            {
                previousVersion.FileStatus = FileStatus.OldVersion;
                file.PreviousVersionId = previousVersion.Id;
            }

            file.ParentFolderId = targetFolder.Id;
            file.MetadataBundle = reqFile.MetadataBundle;
            file.KeyBundle = reqFile.KeyBundle;
            file.MaterializedPath = $"{targetFolder.MaterializedPath}{file.Id}";
        }

        await ctx.SaveChangesAsync(ct);

        return TypedResults.Ok();
    }
}

public sealed record NodeToMove(Guid Id, MetadataBundle MetadataBundle, SecretKeyBundle KeyBundle);

public sealed record FileNodeToMove(Guid Id, MetadataBundle MetadataBundle,
    SecretKeyBundle KeyBundle,
    Guid? PreviousVersionId);

public sealed record MoveRequest(Guid TargetFolderId, NodeToMove[] Folders, FileNodeToMove[] Files);

public sealed class MoveRequestValidator : AbstractValidator<MoveRequest>
{
    public MoveRequestValidator()
    {
        RuleFor(x => x.TargetFolderId).NotEmpty();
        RuleFor(x => x.Folders).NotNull();
        RuleFor(x => x.Files).NotNull();
    }
}