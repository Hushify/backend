using Hushify.Api.Features.Drive.Entities;
using Hushify.Api.Features.Identity.Entities;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Hushify.Api.Persistence;

public class AppDbContext : IdentityDbContext<AppUser, AppRole, Guid>, IDataProtectionKeyContext
{
    private readonly ILogger<AppDbContext> _logger;
    private IDbContextTransaction? _currentTransaction;

    protected AppDbContext(DbContextOptions options, ILogger<AppDbContext> logger) :
        base(options) => _logger = logger;

    public AppDbContext(DbContextOptions<AppDbContext> options, ILogger<AppDbContext> logger) :
        base(options) => _logger = logger;

    public DbSet<Workspace> Workspaces => Set<Workspace>();
    public DbSet<FileNode> Files => Set<FileNode>();
    public DbSet<FolderNode> Folders => Set<FolderNode>();
    public DbSet<DataProtectionKey> DataProtectionKeys => Set<DataProtectionKey>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<AppUser>().HasIndex(x => x.WorkspaceId);
        builder.Entity<AppUser>().Property(x => x.CryptoProperties).HasColumnType("jsonb");

        builder.Entity<AppRole>().HasIndex(x => x.NormalizedName).IsUnique(false);
        builder.Entity<AppRole>().HasIndex(x => x.WorkspaceId);
        builder.Entity<AppRole>().HasIndex(x => new { x.NormalizedName, x.WorkspaceId }).IsUnique();

        builder.Entity<FileNode>().Property(x => x.KeyBundle).HasColumnType("jsonb");
        builder.Entity<FileNode>().Property(x => x.MetadataBundle).HasColumnType("jsonb");
        builder.Entity<FileNode>().Property(x => x.FileS3Config).HasColumnType("jsonb");
        builder.Entity<FileNode>().HasIndex(x => x.MaterializedPath);
        builder.Entity<FileNode>().HasIndex(x => new { x.Id, x.WorkspaceId }).IsUnique();
        builder.Entity<FileNode>().HasIndex(x => x.WorkspaceId);
        builder.Entity<FileNode>().Property(x => x.FileStatus).HasConversion<string>();

        builder.Entity<FolderNode>().Property(x => x.KeyBundle).HasColumnType("jsonb");
        builder.Entity<FolderNode>().Property(x => x.MetadataBundle).HasColumnType("jsonb");
        builder.Entity<FolderNode>().HasIndex(x => x.MaterializedPath);
        builder.Entity<FolderNode>().HasIndex(x => new { x.Id, x.WorkspaceId }).IsUnique();
        builder.Entity<FolderNode>().HasIndex(x => x.WorkspaceId);
        builder.Entity<FolderNode>().Property(x => x.FolderStatus).HasConversion<string>();
    }

    public async Task BeginTransactionAsync()
    {
        if (_currentTransaction is not null)
        {
            _logger.LogInformation("A transaction with ID {ID} is already created",
                _currentTransaction.TransactionId);
            return;
        }


        _currentTransaction = await Database.BeginTransactionAsync();
        _logger.LogInformation("A new transaction was created with ID {ID}",
            _currentTransaction.TransactionId);
    }

    public async Task CommitTransactionAsync()
    {
        if (_currentTransaction is null)
        {
            return;
        }

        _logger.LogInformation("Commiting Transaction {ID}", _currentTransaction.TransactionId);

        await _currentTransaction.CommitAsync();

        _currentTransaction.Dispose();
        _currentTransaction = null;
    }

    public async Task RollbackTransaction()
    {
        if (_currentTransaction is null)
        {
            return;
        }

        _logger.LogDebug("Rolling back Transaction {ID}", _currentTransaction.TransactionId);

        await _currentTransaction.RollbackAsync();

        _currentTransaction.Dispose();
        _currentTransaction = null;
    }
}