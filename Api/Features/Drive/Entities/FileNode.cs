using Hushify.Api.Features.Identity.Entities;
using Hushify.Api.Filters;

namespace Hushify.Api.Features.Drive.Entities;

public sealed class FileNode : IWorkspaceFilter
{
    private FileNode() { }

    public FileNode(Guid id, string materializedPath, Guid workspaceId, Guid parentFolderId, FileS3Config fileS3Config,
        SecretKeyBundle fileKeyBundle,
        MetadataBundle metadataBundle)
    {
        Id = id;
        MaterializedPath = materializedPath;
        WorkspaceId = workspaceId;
        FileKeyBundle = fileKeyBundle;
        MetadataBundle = metadataBundle;
        ParentFolderId = parentFolderId;
        FileS3Config = fileS3Config;
    }

    public Guid Id { get; set; } = Guid.NewGuid();
    public string MaterializedPath { get; set; } = default!;
    public SecretKeyBundle FileKeyBundle { get; set; } = default!;
    public MetadataBundle MetadataBundle { get; set; } = default!;

    public FileS3Config FileS3Config { get; set; } = default!;
    public long EncryptedSize { get; set; }
    public FileStatus FileStatus { get; set; } = FileStatus.UploadStarted;
    public Workspace Workspace { get; set; } = default!;

    public Guid? ParentFolderId { get; set; }
    public FolderNode? ParentFolder { get; set; }

    public Guid? PreviousVersionId { get; set; }
    public FileNode? PreviousVersion { get; set; }

    public Guid WorkspaceId { get; set; }
}