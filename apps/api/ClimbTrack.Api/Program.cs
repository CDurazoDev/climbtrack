using System.Security.Claims;
using System.Text;
using ClimbTrack.Api.Configuration;
using ClimbTrack.Api.Endpoints.Auth;
using ClimbTrack.Api.Endpoints.Catalogs;
using ClimbTrack.Api.Endpoints.CustomSessions;
using ClimbTrack.Api.Endpoints.SessionLogs;
using ClimbTrack.Api.Endpoints.Stats;
using ClimbTrack.Api.Endpoints.Users;
using ClimbTrack.Application;
using ClimbTrack.Infrastructure;
using ClimbTrack.Infrastructure.Persistence;
using ClimbTrack.Infrastructure.Persistence.Seeds;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Context;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

LoggingConfiguration.ConfigureStructuredLogging(builder);

var jwtSecret = builder.Configuration["Jwt:Secret"];
if (string.IsNullOrWhiteSpace(jwtSecret) || jwtSecret == "CHANGE_ME")
{
    throw new InvalidOperationException("Jwt:Secret must be configured from a secure local source.");
}

builder.Services.AddOpenApi();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    await using var scope = app.Services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ClimbTrackDbContext>();
    await CatalogSeeder.SeedAsync(dbContext);
}

app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate =
        "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    options.GetLevel = (httpContext, elapsed, ex) =>
        ex is not null || httpContext.Response.StatusCode >= StatusCodes.Status500InternalServerError
            ? LogEventLevel.Error
            : LogEventLevel.Information;
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("requestPath", httpContext.Request.Path.Value ?? string.Empty);
        diagnosticContext.Set("httpMethod", httpContext.Request.Method);
        diagnosticContext.Set("statusCode", httpContext.Response.StatusCode);
        diagnosticContext.Set("traceId", httpContext.TraceIdentifier);
        diagnosticContext.Set(
            "correlationId",
            httpContext.Request.Headers["X-Correlation-ID"].FirstOrDefault() ?? httpContext.TraceIdentifier);
        diagnosticContext.Set(
            "userId",
            httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
    };
});
app.UseAuthentication();
app.Use(async (context, next) =>
{
    using var requestPathScope = LogContext.PushProperty("requestPath", context.Request.Path.Value ?? string.Empty);
    using var httpMethodScope = LogContext.PushProperty("httpMethod", context.Request.Method);
    using var traceIdScope = LogContext.PushProperty("traceId", context.TraceIdentifier);
    using var correlationIdScope = LogContext.PushProperty(
        "correlationId",
        context.Request.Headers["X-Correlation-ID"].FirstOrDefault() ?? context.TraceIdentifier);
    using var userIdScope = LogContext.PushProperty(
        "userId",
        context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

    await next();
});
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "healthy" })).AllowAnonymous();
app.MapGroup("/auth").MapAuth();
app.MapGroup("/catalogs").MapCatalogs();
app.MapGroup("/custom-sessions").MapCustomSessions();
app.MapGroup("/session-logs").MapSessionLogs();
app.MapGroup("/stats").MapStats();
app.MapGroup("/users").MapUsers();

app.Run();
