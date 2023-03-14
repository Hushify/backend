using Amazon.CloudFront;
using Amazon.S3;
using Amazon.S3.Model;
using Hushify.Api.Exceptions;
using Hushify.Api.Features.Drive.Endpoints;
using Hushify.Api.Features.Drive.Entities;
using Hushify.Api.Features.Identity.Entities;
using Hushify.Api.Features.Identity.Services;
using Hushify.Api.Options;
using Hushify.Api.Persistence;
using Hushify.Api.Providers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Hushify.Api.Features.Drive.Services;

public interface IDriveService
{
    Task<ListResponse> ListAsync(Guid? currentFolderId, CancellationToken cancellationToken);
    Task<(long Total, long Used)> GetDriveStatsAsync(CancellationToken cancellationToken);

    Task<CreateMultipartUploadResponse> PrepareForMultipartUploadAsync(Guid parentFolderId, Guid? previousVersionId,
        int numberOfChunks,
        long encryptedSize,
        SecretKeyBundle keyBundle, MetadataBundle metadataBundle, CancellationToken cancellationToken);
}

public sealed class DriveS3Service : IDriveService
{
    private readonly AWSOptions _awsOptions;
    private readonly WorkspaceDbContext _context;
    private readonly CryptoKeys _cryptoKeys;
    private readonly IAmazonS3 _s3Client;
    private readonly IWorkspaceProvider _workspaceProvider;

    public DriveS3Service(WorkspaceDbContext context, IAmazonS3 s3Client, IOptions<ConfigOptions> options,
        IWorkspaceProvider workspaceProvider, CryptoKeys cryptoKeys)
    {
        _context = context;
        _s3Client = s3Client;
        _workspaceProvider = workspaceProvider;
        _cryptoKeys = cryptoKeys;
        _awsOptions = options.Value.AWS;
    }

    public async Task<ListResponse> ListAsync(Guid? currentFolderId, CancellationToken cancellationToken)
    {
        var workspaceFolder = await _context.Folders.FirstOrDefaultAsync(
            f => f.ParentFolderId == null,
            cancellationToken) ?? throw new AppException("Workspace folder is missing.");

        currentFolderId ??= workspaceFolder.Id;

        var breadcrumbs = new List<BreadcrumbVM>();

        var currentFolder = await _context.Folders
            .AsNoTracking()
            .Include(f => f.Folders)
            .Include(f => f.Files)
            .Include(f => f.ParentFolder)
            .FirstOrDefaultAsync(f => f.Id == currentFolderId && f.FolderStatus == FolderStatus.Normal,
                cancellationToken) ?? throw new AppException("Folder does not exists.");

        var files = currentFolder.Files
            .Where(f => f.FileStatus == FileStatus.UploadFinished)
            .Select(f => new FileNodeVM(f.Id, f.MetadataBundle, f.KeyBundle,
                _awsOptions.IsCloudFrontEnabled
                    ? GenerateCloudFrontUrl(f.FileS3Config.Key, DateTime.UtcNow.AddHours(24))
                    : GenerateGetObjectUrl(f.FileS3Config.Key, f.FileS3Config.BucketName,
                        DateTime.UtcNow.AddHours(24)), f.IsShared));

        var folders = currentFolder.Folders
            .Where(f => f.FolderStatus == FolderStatus.Normal)
            .Select(f => new FolderNodeVM(f.Id, f.MetadataBundle, f.KeyBundle, f.IsShared));

        if (currentFolder.ParentFolderId is null)
        {
            return new ListResponse(workspaceFolder.Id, currentFolder.Id, breadcrumbs, files, folders);
        }

        var current = currentFolder;
        while (current is not null)
        {
            breadcrumbs.Add(new BreadcrumbVM(current.Id, current.MetadataBundle, current.KeyBundle, current.IsShared));
            current = await _context.Folders
                .Include(sf => sf.ParentFolder)
                .FirstOrDefaultAsync(sf => sf.Id == current.ParentFolderId
                                           && sf.ParentFolderId != null, cancellationToken);
        }

        breadcrumbs.Reverse();

        return new ListResponse(workspaceFolder.Id, currentFolder.Id, breadcrumbs, files, folders);
    }

    public async Task<(long Total, long Used)> GetDriveStatsAsync(CancellationToken cancellationToken)
    {
        var total = await _context.Workspaces.Where(w => w.Id == _workspaceProvider.GetWorkspaceId())
            .Select(w => w.StorageSize).FirstOrDefaultAsync(cancellationToken);

        var used = await _context.Files
            .Where(node =>
                node.FileStatus == FileStatus.UploadFinished ||
                node.FileStatus == FileStatus.Deleted)
            .SumAsync(so => so.EncryptedSize, cancellationToken);

        return (total, used);
    }

    public async Task<CreateMultipartUploadResponse> PrepareForMultipartUploadAsync(Guid parentFolderId,
        Guid? previousVersionId,
        int numberOfChunks,
        long encryptedSize,
        SecretKeyBundle keyBundle, MetadataBundle metadataBundle, CancellationToken cancellationToken)
    {
        var storageStats = await GetDriveStatsAsync(cancellationToken);

        if (storageStats.Used > storageStats.Total || storageStats.Used + encryptedSize > storageStats.Total)
        {
            throw new AppException("You have already used up all your storage.");
        }

        var parentFolder =
            await _context.Folders.FirstOrDefaultAsync(node => node.Id == parentFolderId, cancellationToken);

        if (parentFolder is null)
        {
            throw new AppException("No parent folder found.");
        }

        var previousVersion =
            await _context.Files.FirstOrDefaultAsync(f => f.Id == previousVersionId, cancellationToken);

        var fileId = Guid.NewGuid();
        var materializedPath = parentFolder.MaterializedPath + fileId;
        var key = GetKey(fileId);

        var fileS3Config = new FileS3Config
        {
            Region = _awsOptions.Region,
            BucketName = _awsOptions.BucketName,
            Key = key
        };

        var file = new FileNode(fileId, materializedPath, _workspaceProvider.GetWorkspaceId(), parentFolder.Id,
            fileS3Config, keyBundle, metadataBundle)
        {
            EncryptedSize = encryptedSize,
            PreviousVersion = previousVersion
        };

        var initResponse = await _s3Client.InitiateMultipartUploadAsync(new InitiateMultipartUploadRequest
        {
            BucketName = fileS3Config.BucketName,
            Key = fileS3Config.Key
        }, cancellationToken);

        file.FileS3Config.UploadId = initResponse.UploadId;

        await _context.Files.AddAsync(file, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        var parts = new List<Part>();
        for (var i = 0; i < numberOfChunks; i++)
        {
            var uploadUrl =
                GenerateMultipartPutObjectUrl(key, fileS3Config.BucketName, file.FileS3Config.UploadId, i + 1,
                    DateTime.UtcNow.AddHours(2));
            var part = new Part(i + 1, uploadUrl);
            parts.Add(part);
        }

        return new CreateMultipartUploadResponse(file.Id, file.FileS3Config.UploadId, parts);
    }

    private string GenerateMultipartPutObjectUrl(string key, string bucketName, string uploadId, int partNumber,
        DateTime expiry) =>
        _s3Client.GetPreSignedURL(new GetPreSignedUrlRequest
        {
            BucketName = bucketName,
            Key = key,
            Expires = expiry,
            Protocol = Protocol.HTTPS,
            Verb = HttpVerb.PUT,
            PartNumber = partNumber,
            UploadId = uploadId
        });

    private string GenerateGetObjectUrl(string key, string bucketName, DateTime expiry) =>
        _s3Client.GetPreSignedURL(new GetPreSignedUrlRequest
        {
            BucketName = bucketName,
            Key = key,
            Expires = expiry,
            Protocol = Protocol.HTTPS,
            Verb = HttpVerb.GET
        });

    private string GenerateCloudFrontUrl(string resource, DateTime expiry) =>
        AmazonCloudFrontUrlSigner.GetCannedSignedURL(
            AmazonCloudFrontUrlSigner.Protocol.https,
            _awsOptions.CloudFrontServiceUrl,
            new StringReader(_cryptoKeys.PrivateSecurityKey.Rsa.ExportRSAPrivateKeyPem()),
            resource, _awsOptions.KeyId, expiry);

    private string GetKey(Guid fileId) => $"workspace-{_workspaceProvider.GetWorkspaceId()}/{fileId}";

    private string GetRootPath() => $"workspace-{_workspaceProvider.GetWorkspaceId()}/";
}