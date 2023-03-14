using Hushify.Api.Persistence;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Hushify.Api.Features.Drive.Endpoints;

public static class Share
{
    public static IEndpointRouteBuilder MapShareEndpoints(this RouteGroupBuilder routes)
    {
        routes.MapPost("/share/{type}/{id:guid}", CreateShareHandler);
        routes.MapPost("/share-stop/{type}/{id:guid}", StopShareHandler);

        return routes;
    }

    private static async Task<Results<Ok, ValidationProblem>> CreateShareHandler(
        string type, Guid id, WorkspaceDbContext ctx, CancellationToken ct)
    {
        switch (type.ToUpper())
        {
            case "FOLDER":
                await ctx.Folders.Where(f => f.Id == id)
                    .ExecuteUpdateAsync(s => s.SetProperty(f => f.IsShared, true), ct);
                return TypedResults.Ok();
            case "FILE":
                await ctx.Files.Where(f => f.Id == id)
                    .ExecuteUpdateAsync(s => s.SetProperty(f => f.IsShared, true), ct);
                return TypedResults.Ok();
        }

        throw new Exception("Invalid node type.");
    }

    private static async Task<Results<Ok, ValidationProblem>> StopShareHandler(string type, Guid id,
        WorkspaceDbContext ctx,
        CancellationToken ct)
    {
        switch (type.ToUpper())
        {
            case "FOLDER":
                await ctx.Folders.Where(f => f.Id == id)
                    .ExecuteUpdateAsync(s => s.SetProperty(f => f.IsShared, false), ct);
                return TypedResults.Ok();
            case "FILE":
                await ctx.Files.Where(f => f.Id == id)
                    .ExecuteUpdateAsync(s => s.SetProperty(f => f.IsShared, false), ct);
                return TypedResults.Ok();
        }

        throw new Exception("Invalid node type.");
    }
}