using Hushify.Api.Providers;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;

namespace Hushify.Api.Persistence;

public static class PersistenceExtensions
{
    public static void AddEfAndDataProtection(this WebApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

        // EF Core + Data Protection
        builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
        builder.Services.AddDataProtection().PersistKeysToDbContext<AppDbContext>();

        // Workspace provider, required for query filters
        builder.Services.AddScoped<IWorkspaceProvider, WorkspaceProvider>();

        // Workspace DbContext - Has Query Filters
        builder.Services.AddDbContext<WorkspaceDbContext>(options => options.UseNpgsql(connectionString));
    }
}