using Hushify.Api.Features.Drive.Entities;
using Hushify.Api.Features.Identity.Entities;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Hushify.Api.Persistence;

public class AppDbContext : IdentityDbContext<AppUser, AppRole, Guid>, IDataProtectionKeyContext
{
    protected AppDbContext(DbContextOptions options) : base(options) { }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Workspace> Workspaces => Set<Workspace>();
    public DbSet<FileNode> Files => Set<FileNode>();
    public DbSet<FolderNode> Folders => Set<FolderNode>();
    public DbSet<DataProtectionKey> DataProtectionKeys => Set<DataProtectionKey>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<AppUser>().Property(x => x.CryptoProperties).HasColumnType("jsonb");

        builder.Entity<AppRole>().HasIndex(x => x.NormalizedName).IsUnique(false);
        builder.Entity<AppRole>().HasIndex(x => new { x.NormalizedName, x.WorkspaceId }).IsUnique();

        builder.Entity<FileNode>().Property(x => x.KeyBundle).HasColumnType("jsonb");
        builder.Entity<FolderNode>().Property(x => x.KeyBundle).HasColumnType("jsonb");

        builder.Entity<FileNode>().Property(x => x.MetadataBundle).HasColumnType("jsonb");
        builder.Entity<FolderNode>().Property(x => x.MetadataBundle).HasColumnType("jsonb");

        builder.Entity<FileNode>().Property(x => x.FileS3Config).HasColumnType("jsonb");
    }
}