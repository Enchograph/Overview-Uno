using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Overview.Server.Infrastructure.Configuration;
using Overview.Server.Infrastructure.Persistence;

namespace Overview.Server.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddServerInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<PersistenceOptions>(
            configuration.GetSection(PersistenceOptions.SectionName));

        var persistenceOptions = configuration
            .GetSection(PersistenceOptions.SectionName)
            .Get<PersistenceOptions>()
            ?? new PersistenceOptions();

        var connectionString = configuration.GetConnectionString(persistenceOptions.ConnectionStringName)
            ?? throw new InvalidOperationException(
                $"Connection string '{persistenceOptions.ConnectionStringName}' was not found.");

        services.AddDbContext<OverviewDbContext>(options =>
            options.UseNpgsql(
                connectionString,
                npgsql => npgsql.MigrationsAssembly(typeof(OverviewDbContext).Assembly.FullName)));

        return services;
    }
}
