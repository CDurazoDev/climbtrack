using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ClimbTrack.Infrastructure.Persistence;

public class ClimbTrackDbContextFactory : IDesignTimeDbContextFactory<ClimbTrackDbContext>
{
    public ClimbTrackDbContext CreateDbContext(string[] args)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        var apiProjectPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "ClimbTrack.Api"));

        var configuration = new ConfigurationBuilder()
            .AddJsonFile(Path.Combine(apiProjectPath, "appsettings.json"), optional: true)
            .AddJsonFile(Path.Combine(apiProjectPath, $"appsettings.{environment}.json"), optional: true)
            .Build();

        var connectionString =
            Environment.GetEnvironmentVariable("CLIMBTRACK_PG")
            ?? configuration.GetConnectionString("PostgreSQL")
            ?? throw new InvalidOperationException(
                "Connection string not found. Configure CLIMBTRACK_PG or ConnectionStrings:PostgreSQL.");

        var optionsBuilder = new DbContextOptionsBuilder<ClimbTrackDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new ClimbTrackDbContext(optionsBuilder.Options);
    }
}
