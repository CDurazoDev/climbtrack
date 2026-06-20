using ClimbTrack.Application.Common.Interfaces;
using ClimbTrack.Application.Common.Security;
using ClimbTrack.Domain.Interfaces;
using ClimbTrack.Infrastructure.Auth;
using ClimbTrack.Infrastructure.Messaging;
using ClimbTrack.Infrastructure.Persistence;
using ClimbTrack.Infrastructure.Persistence.Connections;
using ClimbTrack.Infrastructure.Persistence.Interceptors;
using ClimbTrack.Infrastructure.Services;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ClimbTrack.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PostgreSQL")
            ?? throw new InvalidOperationException("ConnectionStrings:PostgreSQL is required.");

        services.TryAddScoped<ICurrentUserService, AnonymousCurrentUserService>();
        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ClimbTrackDbContext>());
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IPasswordResetTokenService, PasswordResetTokenService>();
        services.AddScoped<IPasswordResetNotificationService, PasswordResetNotificationService>();
        services.AddScoped<IEmailSender, SmtpEmailSender>();
        services.AddScoped<RlsInterceptor>();
        services.AddScoped<ISqlConnectionFactory>(sp =>
            new NpgsqlSqlConnectionFactory(connectionString, sp.GetRequiredService<ICurrentUserService>()));
        var hangfireOptions = new PostgreSqlStorageOptions
        {
            PrepareSchemaIfNecessary = true
        };
        GlobalConfiguration.Configuration.UsePostgreSqlStorage(
            options => options.UseNpgsqlConnection(connectionString, _ => { }),
            hangfireOptions);
        services.AddSingleton<JobStorage>(_ => JobStorage.Current);
        services.AddSingleton<IRecurringJobManager>(sp =>
            new RecurringJobManager(sp.GetRequiredService<JobStorage>()));

        services.AddDbContext<ClimbTrackDbContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString);
            options.AddInterceptors(sp.GetRequiredService<RlsInterceptor>());
        });

        return services;
    }
}
