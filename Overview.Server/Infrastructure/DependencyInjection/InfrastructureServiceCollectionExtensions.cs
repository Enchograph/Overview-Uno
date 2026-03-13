using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Overview.Server.Infrastructure.Configuration;

namespace Overview.Server.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddServerInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<PersistenceOptions>(
            configuration.GetSection(PersistenceOptions.SectionName));

        return services;
    }
}
