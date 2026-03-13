using Microsoft.Extensions.DependencyInjection;

namespace Overview.Server.Application.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddServerApplication(this IServiceCollection services)
    {
        return services;
    }
}
