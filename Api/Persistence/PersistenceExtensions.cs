using Hushify.Api.Features.Drive.Entities;
using Hushify.Api.Providers;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Hushify.Api.Persistence;

public static class PersistenceExtensions
{
    public static void AddEfAndDataProtection(this WebApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        dataSourceBuilder.MapEnum<FileStatus>().MapEnum<FolderStatus>();
        var dataSource = dataSourceBuilder.Build();

        // EF Core + Data Protection
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(dataSource)
                .EnableSensitiveDataLogging(builder.Environment.IsDevelopment()));
        builder.Services.AddDataProtection().PersistKeysToDbContext<AppDbContext>();

        // Workspace provider, required for query filters
        builder.Services.AddScoped<IWorkspaceProvider, WorkspaceProvider>();

        // Workspace DbContext - Has Query Filters
        builder.Services.AddDbContext<WorkspaceDbContext>(options =>
            options.UseNpgsql(dataSource)
                .EnableSensitiveDataLogging(builder.Environment.IsDevelopment()));
    }
}