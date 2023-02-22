using Amazon.S3;
using Amazon.S3.Model;
using Hushify.Api.Exceptions;
using Hushify.Api.Features.Drive.Endpoints;
using Hushify.Api.Features.Drive.Entities;
using Hushify.Api.Options;
using Hushify.Api.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Hushify.Api.Features.Drive.Services;

public sealed class DriveS3Service : IDriveService
{
    private readonly AWSOptions _awsOptions;
    private readonly WorkspaceDbContext _context;
    private readonly IAmazonS3 _s3Client;

    public DriveS3Service(WorkspaceDbContext context, IAmazonS3 s3Client, IOptions<ConfigOptions> options)
    {
        _context = context;
        _s3Client = s3Client;
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
            .Select(f => new FileNodeVM(f.Id, f.MetadataBundle, f.EncryptedSize, f.FileKeyBundle,
                GenerateGetObjectUrl(f.Key, DateTime.UtcNow.AddHours(24))));

        var folders = currentFolder.Folders
            .Where(f => f.FolderStatus == FolderStatus.Normal)
            .Select(f => new FolderNodeVM(f.Id, f.MetadataBundle, f.FolderKeyBundle));

        if (currentFolder.ParentFolderId is null)
        {
            return new ListResponse(workspaceFolder.Id, currentFolder.Id, breadcrumbs, files, folders);
        }

        var current = currentFolder;
        while (current is not null)
        {
            breadcrumbs.Add(new BreadcrumbVM(current.Id, current.MetadataBundle, current.FolderKeyBundle));
            current = await _context.Folders
                .Include(sf => sf.ParentFolder)
                .FirstOrDefaultAsync(sf => sf.Id == current.ParentFolderId
                                           && sf.ParentFolderId != null, cancellationToken);
        }

        breadcrumbs.Reverse();

        return new ListResponse(workspaceFolder.Id, currentFolder.Id, breadcrumbs, files, folders);
    }

    private string GenerateGetObjectUrl(string key, DateTime expiry) =>
        _s3Client.GetPreSignedURL(new GetPreSignedUrlRequest
        {
            BucketName = _awsOptions.BucketName,
            Key = key,
            Expires = expiry,
            Protocol = Protocol.HTTPS,
            Verb = HttpVerb.GET
        });
}