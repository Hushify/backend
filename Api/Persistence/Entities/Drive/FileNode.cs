using Hushify.Api.Persistence.Filters;

namespace Hushify.Api.Persistence.Entities.Drive;

public sealed class FileNode : IWorkspaceFilter
{
    private FileNode() { }

    public FileNode(string materializedPath, Guid workspaceId, SecretKeyBundle fileKeyBundle,
        MetadataBundle? metadataBundle,
        Guid? parentFolderId)
    {
        MaterializedPath = materializedPath;
        WorkspaceId = workspaceId;
        FileKeyBundle = fileKeyBundle;
        MetadataBundle = metadataBundle;
        ParentFolderId = parentFolderId;
    }

    public Guid Id { get; set; } = Guid.NewGuid();
    public string MaterializedPath { get; set; } = default!;
    public SecretKeyBundle FileKeyBundle { get; set; } = default!;
    public MetadataBundle? MetadataBundle { get; set; }

    public string Region { get; set; } = default!;
    public string BucketName { get; set; } = default!;
    public string Key { get; set; } = default!;
    public long EncryptedSize { get; set; }
    public UploadStatus UploadStatus { get; set; } = UploadStatus.UploadStarted;
    public Workspace Workspace { get; set; } = default!;

    public Guid? ParentFolderId { get; set; }
    public FolderNode? ParentFolder { get; set; }

    public Guid WorkspaceId { get; set; }
}