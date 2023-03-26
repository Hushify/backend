using Hushify.Api.Exceptions;
using Hushify.Api.Features.Drive.Entities;
using Hushify.Api.Features.Identity.Entities;
using Hushify.Api.Filters;
using Hushify.Api.Providers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Hushify.Api.Persistence;

public sealed class WorkspaceDbContext : AppDbContext
{
    private readonly IWorkspaceProvider _workspaceProvider;

    public WorkspaceDbContext(DbContextOptions options, IWorkspaceProvider workspaceProvider) :
        base(options) => _workspaceProvider = workspaceProvider;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // TODO: Auto-attach workspace query filter
        // Ref: https://www.thereformedprogrammer.net/building-asp-net-core-and-ef-core-multi-tenant-apps-part1-the-database/
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if ((entityType.ClrType.Namespace ?? string.Empty).StartsWith(typeof(IApiMarker)
                    .Namespace!) &&
                !typeof(IWorkspaceFilter).IsAssignableFrom(entityType.ClrType) &&
                !typeof(ISkipWorkspaceFilter).IsAssignableFrom(entityType.ClrType))
            {
                throw new AppException(
                    $"Found entity without IWorkspaceFilter applied: {entityType.ClrType.ShortDisplayName()}");
            }
        }

        base.OnModelCreating(builder);

        builder.Entity<AppUser>()
            .HasQueryFilter(x => x.WorkspaceId == _workspaceProvider.GetWorkspaceId());

        builder.Entity<AppRole>()
            .HasQueryFilter(x => x.WorkspaceId == _workspaceProvider.GetWorkspaceId());

        builder.Entity<FileNode>()
            .HasQueryFilter(x => x.WorkspaceId == _workspaceProvider.GetWorkspaceId());

        builder.Entity<FolderNode>()
            .HasQueryFilter(x => x.WorkspaceId == _workspaceProvider.GetWorkspaceId());
    }
}