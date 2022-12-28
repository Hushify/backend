using Hushify.Api.Exceptions;
using Hushify.Api.Persistence.Configurations;
using Hushify.Api.Persistence.Entities;
using Hushify.Api.Persistence.Entities.Drive;
using Hushify.Api.Persistence.Filters;
using Hushify.Api.Persistence.Providers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Hushify.Api.Persistence;

public sealed class WorkspaceDbContext : AppDbContext
{
    private readonly IEnumerable<EntityTypeConfigurationDependency> _configurations;
    private readonly IWorkspaceProvider _workspaceProvider;

    public WorkspaceDbContext(DbContextOptions options, IEnumerable<EntityTypeConfigurationDependency> configurations,
        IWorkspaceProvider workspaceProvider) :
        base(options)
    {
        _configurations = configurations;
        _workspaceProvider = workspaceProvider;
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // TODO: Automatically add workspace query filter
        // Ref: https://www.thereformedprogrammer.net/building-asp-net-core-and-ef-core-multi-tenant-apps-part1-the-database/
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if ((entityType.ClrType.Namespace ?? string.Empty).StartsWith(typeof(IApiMarker).Namespace!) &&
                !typeof(IWorkspaceFilter).IsAssignableFrom(entityType.ClrType) &&
                !typeof(ISkipWorkspaceFilter).IsAssignableFrom(entityType.ClrType))
            {
                throw new AppException(
                    $"Found entity without IWorkspaceFilter applied: {entityType.ClrType.ShortDisplayName()}");
            }
        }

        base.OnModelCreating(builder);

        // https://github.com/dotnet/efcore/issues/23103
        foreach (var entityTypeConfiguration in _configurations)
        {
            entityTypeConfiguration.Configure(builder);
        }

        builder.Entity<AppUser>().HasQueryFilter(x => x.WorkspaceId == _workspaceProvider.GetWorkspaceId());
        builder.Entity<AppRole>().HasQueryFilter(x => x.WorkspaceId == _workspaceProvider.GetWorkspaceId());
        builder.Entity<FileNode>().HasQueryFilter(x => x.WorkspaceId == _workspaceProvider.GetWorkspaceId());
        builder.Entity<FolderNode>().HasQueryFilter(x => x.WorkspaceId == _workspaceProvider.GetWorkspaceId());
    }
}