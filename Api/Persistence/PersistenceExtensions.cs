using Hushify.Api.Persistence.Configurations;
using Hushify.Api.Persistence.Providers;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;

namespace Hushify.Api.Persistence;

public static class PersistenceExtensions
{
    public static IServiceCollection AddEfAndDataProtection(this IServiceCollection services,
        IConfiguration configuration)
    {
        // EF Core + Data Protection
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"))
        );

        services.AddDataProtection().PersistKeysToDbContext<AppDbContext>();

        services.AddDbContext<WorkspaceDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"))
        );

        // Entity Configurations
        foreach (var type in typeof(AppDbContext).Assembly.DefinedTypes.Where(t =>
                     t is { IsAbstract: false, IsGenericTypeDefinition: false } &&
                     typeof(EntityTypeConfigurationDependency).IsAssignableFrom(t)))
        {
            services.AddScoped(typeof(EntityTypeConfigurationDependency), type);
        }

        // Workspace provider, required for query filters
        services.AddScoped<IWorkspaceProvider, WorkspaceProvider>();

        return services;
    }
}