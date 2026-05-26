using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ClimbTrack.Infrastructure.Persistence;

public class ClimbTrackDbContextFactory : IDesignTimeDbContextFactory<ClimbTrackDbContext>
{
    public ClimbTrackDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("CLIMBTRACK_PG") ??
            "Host=localhost;Port=5432;Database=YOUR_DATABASE;Username=YOUR_USER;Password=YOUR_PASSWORD";

        var optionsBuilder = new DbContextOptionsBuilder<ClimbTrackDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new ClimbTrackDbContext(optionsBuilder.Options);
    }
}
