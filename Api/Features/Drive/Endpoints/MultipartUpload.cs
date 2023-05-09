using Amazon.S3;
using Amazon.S3.Model;
using FluentValidation;
using Hushify.Api.Exceptions;
using Hushify.Api.Features.Drive.Entities;
using Hushify.Api.Features.Drive.Services;
using Hushify.Api.Features.Identity.Entities;
using Hushify.Api.Filters;
using Hushify.Api.Persistence;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using PartETag = Amazon.S3.Model.PartETag;

namespace Hushify.Api.Features.Drive.Endpoints;

public static class MultipartUpload
{
    public static IEndpointRouteBuilder MapMultipartUploadEndpoints(this RouteGroupBuilder routes)
    {
        routes.WithParameterValidation(typeof(CreateMultipartUploadRequest),
            typeof(CommitMultipartUploadRequest));

        routes.MapPost("/create-multipart-upload", CreateMultipartUploadHandler);
        routes.MapPost("/commit-multipart-upload/{id:guid}", CommitMultipartUploadHandler);
        routes.MapPost("/cancel-multipart-upload/{id:guid}", CancelMultipartUploadHandler);
        return routes;
    }

    private static async Task<Results<Ok<CreateMultipartUploadResponse>, ValidationProblem>>
        CreateMultipartUploadHandler(
            CreateMultipartUploadRequest req, WorkspaceDbContext workspaceDbContext,
            IDriveService driveService,
            CancellationToken ct)
    {
        var response = await driveService.PrepareForMultipartUploadAsync(req.ParentFolderId,
            req.PreviousVersionId,
            req.NumberOfChunks,
            req.EncryptedSize,
            req.KeyBundle, req.MetadataBundle, ct);

        return TypedResults.Ok(response);
    }

    private static async Task<Results<Ok, ValidationProblem>>
        CommitMultipartUploadHandler(
            Guid id, CommitMultipartUploadRequest req, WorkspaceDbContext ctx, IAmazonS3 s3,
            CancellationToken ct)
    {
        var file = await ctx.Files.Include(f => f.PreviousVersion)
            .FirstOrDefaultAsync(f => f.Id == id, ct);
        if (file is null)
        {
            throw new AppException("File doesn't exists.");
        }

        file.FileStatus = FileStatus.UploadFinished;

        if (file.PreviousVersion is not null)
        {
            file.PreviousVersion.FileStatus = FileStatus.OldVersion;
        }

        await s3.CompleteMultipartUploadAsync(new CompleteMultipartUploadRequest
        {
            BucketName = file.FileS3Config.BucketName,
            Key = file.FileS3Config.Key,
            UploadId = file.FileS3Config.UploadId,
            PartETags = req.Parts.Select(p => new PartETag(p.PartNumber, p.ETag)).ToList()
        }, ct);

        await ctx.SaveChangesAsync(ct);

        return TypedResults.Ok();
    }

    private static async Task<Results<Ok, ValidationProblem>>
        CancelMultipartUploadHandler(
            Guid id, WorkspaceDbContext ctx, IAmazonS3 s3,
            CancellationToken ct)
    {
        var file = await ctx.Files.Include(f => f.PreviousVersion)
            .FirstOrDefaultAsync(f => f.Id == id, ct);
        if (file is null)
        {
            throw new AppException("File doesn't exists.");
        }

        file.FileStatus = FileStatus.UploadCancelled;

        await ctx.SaveChangesAsync(ct);

        return TypedResults.Ok();
    }
}

public sealed record CreateMultipartUploadRequest(Guid ParentFolderId, Guid? PreviousVersionId,
    int NumberOfChunks,
    long EncryptedSize, MetadataBundle MetadataBundle, SecretKeyBundle KeyBundle);

public sealed class
    CreateMultipartUploadRequestValidator : AbstractValidator<CreateMultipartUploadRequest>
{
    public CreateMultipartUploadRequestValidator()
    {
        RuleFor(x => x.ParentFolderId).NotEmpty();
        RuleFor(x => x.EncryptedSize).GreaterThan(0);
        RuleFor(x => x.NumberOfChunks).GreaterThan(0);
        RuleFor(x => x.MetadataBundle).NotNull();
        RuleFor(x => x.KeyBundle).NotNull();
    }
}

public sealed record Part(int PartNumber, string PreSignedUrl);

public sealed record CreateMultipartUploadResponse(Guid FileId, string UploadId,
    IEnumerable<Part> Parts);

public sealed record CommitPart(int PartNumber, string ETag);

public sealed record CommitMultipartUploadRequest(IEnumerable<CommitPart> Parts);

public sealed class
    CommitMultipartUploadRequestValidator : AbstractValidator<CommitMultipartUploadRequest>
{
    public CommitMultipartUploadRequestValidator()
    {
        RuleFor(x => x.Parts).NotEmpty();
    }
}