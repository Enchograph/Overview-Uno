using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Overview.Server.Infrastructure.Configuration;
using Overview.Server.Infrastructure.Identity;
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
        services.Configure<AuthenticationOptions>(
            configuration.GetSection(AuthenticationOptions.SectionName));
        services.Configure<SyncOptions>(
            configuration.GetSection(SyncOptions.SectionName));

        var persistenceOptions = configuration
            .GetSection(PersistenceOptions.SectionName)
            .Get<PersistenceOptions>()
            ?? new PersistenceOptions();
        var authenticationOptions = configuration
            .GetSection(AuthenticationOptions.SectionName)
            .Get<AuthenticationOptions>()
            ?? new AuthenticationOptions();

        var connectionString = configuration.GetConnectionString(persistenceOptions.ConnectionStringName)
            ?? throw new InvalidOperationException(
                $"Connection string '{persistenceOptions.ConnectionStringName}' was not found.");

        services.AddDbContext<OverviewDbContext>(options =>
            options.UseNpgsql(
                connectionString,
                npgsql => npgsql.MigrationsAssembly(typeof(OverviewDbContext).Assembly.FullName)));

        var signingKey = Encoding.UTF8.GetBytes(authenticationOptions.JwtSigningKey);
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ValidIssuer = authenticationOptions.Issuer,
                    ValidAudience = authenticationOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(signingKey),
                    ClockSkew = TimeSpan.FromMinutes(1)
                };
            });

        services.AddSingleton(TimeProvider.System);
        services.AddScoped<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddScoped<IAuthTokenService, JwtAuthTokenService>();
        services.AddScoped<IVerificationCodeService, VerificationCodeService>();

        return services;
    }
}
