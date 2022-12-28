using Hushify.Api.Persistence.Filters;

namespace Hushify.Api.Persistence.Entities.Drive;

public sealed class FolderNode : IWorkspaceFilter
{
    private FolderNode() { }

    public FolderNode(Guid id, string materializedPath, Guid workspaceId, SecretKeyBundle folderKeyBundle,
        MetadataBundle? metadataBundle, Guid? parentFolderId)
    {
        Id = id;
        MaterializedPath = materializedPath;
        WorkspaceId = workspaceId;
        FolderKeyBundle = folderKeyBundle;
        MetadataBundle = metadataBundle;
        ParentFolderId = parentFolderId;
    }

    public Guid Id { get; set; } = Guid.NewGuid();
    public string MaterializedPath { get; set; } = default!;
    public SecretKeyBundle FolderKeyBundle { get; set; } = default!;
    public MetadataBundle? MetadataBundle { get; set; }

    public FolderStatus FolderStatus { get; set; } = FolderStatus.Normal;
    public Workspace Workspace { get; set; } = default!;

    public Guid? ParentFolderId { get; set; }
    public FolderNode? ParentFolder { get; set; }

    public ICollection<FileNode> Files { get; set; } = new HashSet<FileNode>();
    public ICollection<FolderNode> Folders { get; set; } = new HashSet<FolderNode>();

    public Guid WorkspaceId { get; set; }
}