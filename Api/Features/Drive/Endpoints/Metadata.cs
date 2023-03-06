using FluentValidation;
using Hushify.Api.Features.Drive.Entities;
using Hushify.Api.Filters;
using Hushify.Api.Persistence;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Hushify.Api.Features.Drive.Endpoints;

public static class Metadata
{
    public static RouteGroupBuilder MapMetadataEndpoints(this RouteGroupBuilder routes)
    {
        routes.WithParameterValidation(typeof(UpdateMetadataRequest));

        routes.MapPost("/update-metadata", UpdateMetadataHandler).AllowAnonymous();

        return routes;
    }

    private static async Task<Results<Ok<UpdateMetadataResponse>, ValidationProblem>> UpdateMetadataHandler(
        UpdateMetadataRequest req, WorkspaceDbContext ctx, CancellationToken ct)
    {
        switch (req.Type.ToUpper())
        {
            case "FOLDER":
                await ctx.Folders.Where(f => f.Id == req.Id)
                    .ExecuteUpdateAsync(s => s.SetProperty(f => f.MetadataBundle, req.MetadataBundle), ct);
                break;
            case "FILE":
                await ctx.Files.Where(f => f.Id == req.Id)
                    .ExecuteUpdateAsync(s => s.SetProperty(f => f.MetadataBundle, req.MetadataBundle), ct);
                break;
            default:
                throw new InvalidOperationException("Invalid node type.");
        }

        return TypedResults.Ok(new UpdateMetadataResponse(req.Id));
    }
}

public sealed record UpdateMetadataRequest(Guid Id, string Type, MetadataBundle MetadataBundle);

public sealed class UpdateMetadataRequestValidator : AbstractValidator<UpdateMetadataRequest>
{
    public UpdateMetadataRequestValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Type).NotEmpty();
        RuleFor(x => x.MetadataBundle).NotNull();
    }
}

public sealed record UpdateMetadataResponse(Guid Id);