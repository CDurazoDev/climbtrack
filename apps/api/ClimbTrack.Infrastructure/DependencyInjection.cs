using ClimbTrack.Application.Common.Interfaces;
using ClimbTrack.Application.Common.Security;
using ClimbTrack.Domain.Interfaces;
using ClimbTrack.Infrastructure.Auth;
using ClimbTrack.Infrastructure.Persistence;
using ClimbTrack.Infrastructure.Persistence.Interceptors;
using ClimbTrack.Infrastructure.Services;
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
        services.TryAddScoped<ICurrentUserService, AnonymousCurrentUserService>();
        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ClimbTrackDbContext>());
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<RlsInterceptor>();

        services.AddDbContext<ClimbTrackDbContext>((sp, options) =>
        {
            options.UseNpgsql(configuration.GetConnectionString("PostgreSQL"));
            options.AddInterceptors(sp.GetRequiredService<RlsInterceptor>());
        });

        return services;
    }
}
