using FluentValidation;
using Hushify.Api.Features.Drive.Entities;
using Hushify.Api.Filters;
using Hushify.Api.Persistence;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Hushify.Api.Features.Drive.Endpoints;

public static class Delete
{
    public static IEndpointRouteBuilder MapDeleteEndpoints(this RouteGroupBuilder routes)
    {
        routes.WithParameterValidation(typeof(DeleteRequest));

        routes.MapPost("/delete-nodes", DeleteFolderHandler);

        return routes;
    }

    private static async Task<Results<Ok<DeleteResponse>, ValidationProblem>> DeleteFolderHandler(
        DeleteRequest req, ILogger<DeleteRequest> logger, WorkspaceDbContext ctx, CancellationToken ct)
    {
        await ctx.Folders.Where(f => req.FolderIds.Contains(f.Id))
            .ExecuteUpdateAsync(s => s.SetProperty(f => f.FolderStatus, FolderStatus.Deleted), ct);

        await ctx.Files.Where(f => req.FileIds.Contains(f.Id))
            .ExecuteUpdateAsync(s => s.SetProperty(f => f.FileStatus, FileStatus.Deleted), ct);

        return TypedResults.Ok(new DeleteResponse(req.FolderIds, req.FileIds));
    }
}

public sealed record DeleteRequest(Guid[] FolderIds, Guid[] FileIds);

public sealed class DeleteRequestValidator : AbstractValidator<DeleteRequest>
{
    public DeleteRequestValidator()
    {
        RuleFor(x => x.FolderIds).NotNull();
        RuleFor(x => x.FileIds).NotNull();
    }
}

public sealed record DeleteResponse(Guid[] FolderIds, Guid[] FileIds);