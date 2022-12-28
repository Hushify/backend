using Hushify.Api.Persistence.Entities.Drive;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hushify.Api.Persistence.Configurations;

public sealed class FileNodeConfiguration : EntityTypeConfigurationDependency<FileNode>
{
    public override void Configure(EntityTypeBuilder<FileNode> builder)
    {
        builder.HasIndex(x => x.MaterializedPath);
        builder.HasIndex(x => new { x.Id, x.WorkspaceId }).IsUnique();
        builder.HasIndex(x => x.Key).IsUnique();
        builder.Property(x => x.UploadStatus).HasConversion<string>();
        builder.HasIndex(x => x.WorkspaceId);
    }
}