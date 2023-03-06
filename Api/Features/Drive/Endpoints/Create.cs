using FluentValidation;
using Hushify.Api.Exceptions;
using Hushify.Api.Features.Drive.Entities;
using Hushify.Api.Features.Identity.Entities;
using Hushify.Api.Filters;
using Hushify.Api.Persistence;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Hushify.Api.Features.Drive.Endpoints;

public static class Create
{
    public static IEndpointRouteBuilder MapCreateEndpoints(this RouteGroupBuilder routes)
    {
        routes.WithParameterValidation(typeof(CreateFolderRequest));

        routes.MapPost("/create-folder", CreateFolderHandler);
        return routes;
    }

    private static async Task<Results<Ok<CreateFolderResponse>, ValidationProblem>> CreateFolderHandler(
        CreateFolderRequest req, WorkspaceDbContext workspaceDbContext,
        CancellationToken ct)
    {
        var workspaceFolder = await workspaceDbContext.Folders.FirstOrDefaultAsync(
            f => f.ParentFolderId == null, ct) ?? throw new AppException("Workspace folder is missing.");

        var parentFolder = workspaceFolder;
        if (req.ParentFolderId is not null)
        {
            parentFolder = await workspaceDbContext.Folders.FirstOrDefaultAsync(
                f => f.Id == req.ParentFolderId, ct) ?? throw new AppException("Parent folder is missing.");
        }

        var folderId = Guid.NewGuid();
        var materializedPath = $"{parentFolder.MaterializedPath}{folderId}/";

        await workspaceDbContext.Folders.AddAsync(
            new FolderNode(folderId, materializedPath, workspaceFolder.Id, req.KeyBundle, req.MetadataBundle,
                parentFolder.Id),
            ct);

        await workspaceDbContext.SaveChangesAsync(ct);

        return TypedResults.Ok(new CreateFolderResponse(folderId));
    }
}

public sealed record CreateFolderRequest(Guid? ParentFolderId, MetadataBundle MetadataBundle,
    SecretKeyBundle KeyBundle);

public sealed class CreateFolderRequestValidator : AbstractValidator<CreateFolderRequest>
{
    public CreateFolderRequestValidator()
    {
        RuleFor(x => x.MetadataBundle).NotNull();
        RuleFor(x => x.KeyBundle).NotNull();
    }
}

public sealed record CreateFolderResponse(Guid Id);