using Hushify.Api.Providers;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;

namespace Hushify.Api.Persistence;

public static class PersistenceExtensions
{
    public static IServiceCollection AddEfAndDataProtection(this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        // EF Core + Data Protection
        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
        services.AddDataProtection().PersistKeysToDbContext<AppDbContext>();
        services.AddDbContext<WorkspaceDbContext>(options => options.UseNpgsql(connectionString));

        // Workspace provider, required for query filters
        services.AddScoped<IWorkspaceProvider, WorkspaceProvider>();

        return services;
    }
}