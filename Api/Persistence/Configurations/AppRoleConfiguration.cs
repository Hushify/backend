using Hushify.Api.Persistence.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hushify.Api.Persistence.Configurations;

public sealed class AppRoleConfiguration : EntityTypeConfigurationDependency<AppRole>
{
    public override void Configure(EntityTypeBuilder<AppRole> builder)
    {
        builder.HasIndex(x => x.NormalizedName).IsUnique(false);
        builder.HasIndex(x => x.WorkspaceId);
        builder.HasIndex(x => new { x.NormalizedName, x.WorkspaceId }).IsUnique();
    }
}