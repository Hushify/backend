using Hushify.Api.Persistence.Entities.Drive;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hushify.Api.Persistence.Configurations;

public sealed class FolderNodeConfiguration : EntityTypeConfigurationDependency<FolderNode>
{
    public override void Configure(EntityTypeBuilder<FolderNode> builder)
    {
        builder.HasIndex(x => x.MaterializedPath);
        builder.HasIndex(x => new { x.Id, x.WorkspaceId }).IsUnique();
        builder.Property(x => x.FolderStatus).HasConversion<string>();
        builder.HasIndex(x => x.WorkspaceId);
    }
}