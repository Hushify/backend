using Hushify.Api.Persistence.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hushify.Api.Persistence.Configurations;

public sealed class AppUserConfiguration : EntityTypeConfigurationDependency<AppUser>
{
    public override void Configure(EntityTypeBuilder<AppUser> builder)
    {
        builder.HasIndex(x => x.WorkspaceId);
    }
}