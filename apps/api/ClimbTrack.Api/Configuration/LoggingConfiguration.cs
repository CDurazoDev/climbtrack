using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace ClimbTrack.Api.Configuration;

public static class LoggingConfiguration
{
    public static void ConfigureStructuredLogging(WebApplicationBuilder builder)
    {
        var logDirectory = ResolveLogDirectory(builder);
        Directory.CreateDirectory(logDirectory);

        var retainedFileCountLimit =
            builder.Configuration.GetValue<int?>("StructuredLogging:RetainedFileCountLimit") ?? 7;
        var fileSizeLimitMb =
            builder.Configuration.GetValue<int?>("StructuredLogging:FileSizeLimitMb") ?? 50;

        builder.Host.UseSerilog((context, services, loggerConfiguration) =>
        {
            loggerConfiguration
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithProperty("service", "ClimbTrack.Api")
                .Enrich.WithProperty("environment", context.HostingEnvironment.EnvironmentName)
                .WriteTo.Console(new RenderedCompactJsonFormatter())
                .WriteTo.File(
                    formatter: new RenderedCompactJsonFormatter(),
                    path: Path.Combine(logDirectory, "api-.json"),
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: retainedFileCountLimit,
                    fileSizeLimitBytes: fileSizeLimitMb * 1024L * 1024L,
                    rollOnFileSizeLimit: true,
                    shared: true);
        });
    }

    public static string ResolveLogDirectory(WebApplicationBuilder builder)
    {
        var configuredPath = builder.Configuration["StructuredLogging:Path"];
        if (!string.IsNullOrWhiteSpace(configuredPath))
        {
            return Path.GetFullPath(configuredPath);
        }

        if (OperatingSystem.IsWindows())
        {
            return builder.Environment.EnvironmentName switch
            {
                "Development" => Path.GetFullPath(
                    Path.Combine(builder.Environment.ContentRootPath, "..", "..", "..", "logs", "api")),
                "Staging" => @"C:\Services\ClimbTrack\logs\api\staging",
                "Production" => @"C:\Services\ClimbTrack\logs\api\production",
                _ => Path.GetFullPath(
                    Path.Combine(builder.Environment.ContentRootPath, "..", "..", "..", "logs", "api"))
            };
        }

        return builder.Environment.EnvironmentName switch
        {
            "Development" => Path.GetFullPath(
                Path.Combine(builder.Environment.ContentRootPath, "..", "..", "..", "logs", "api")),
            "Staging" => "/var/log/climbtrack/api/staging",
            "Production" => "/var/log/climbtrack/api/production",
            _ => Path.GetFullPath(
                Path.Combine(builder.Environment.ContentRootPath, "..", "..", "..", "logs", "api"))
        };
    }
}
