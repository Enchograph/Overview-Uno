using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Overview.Server.Infrastructure.Configuration;

namespace Overview.Server.Infrastructure.Persistence;

public sealed class OverviewDbContextDesignTimeFactory : IDesignTimeDbContextFactory<OverviewDbContext>
{
    public OverviewDbContext CreateDbContext(string[] args)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        var basePath = Directory.GetCurrentDirectory();

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var persistenceOptions = configuration
            .GetSection(PersistenceOptions.SectionName)
            .Get<PersistenceOptions>()
            ?? new PersistenceOptions();

        var connectionString = configuration.GetConnectionString(persistenceOptions.ConnectionStringName)
            ?? "Host=127.0.0.1;Port=5432;Database=overview;Username=overview;Password=change-me";

        var optionsBuilder = new DbContextOptionsBuilder<OverviewDbContext>();
        optionsBuilder.UseNpgsql(connectionString, npgsql => npgsql.MigrationsAssembly(typeof(OverviewDbContext).Assembly.FullName));

        return new OverviewDbContext(optionsBuilder.Options);
    }
}
