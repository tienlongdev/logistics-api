using Logistics.Api.BuildingBlocks.Observability.Observability.Correlation;
using Logistics.Api.BuildingBlocks.Observability.Observability.Logging;
using Logistics.Api.BuildingBlocks.Observability.Observability.OpenTelemetry;
using Logistics.Api.Host.Extensions;
using Logistics.Api.Host.Middleware;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// Serilog (structured logging)
// ============================================================
Log.Logger = SerilogBootstrapper
    .CreateLoggerConfiguration(builder.Configuration, applicationName: "Logistics.Api.Host")
    .CreateLogger();

builder.Host.UseSerilog();

// ============================================================
// Options
// ============================================================
var correlationOptions = builder.Configuration.GetSection(CorrelationIdOptions.SectionName).Get<CorrelationIdOptions>()
    ?? new CorrelationIdOptions();

var otelOptions = builder.Configuration.GetSection(OpenTelemetryOptions.SectionName).Get<OpenTelemetryOptions>()
    ?? new OpenTelemetryOptions();

// ============================================================
// Services
// ============================================================
builder.Services.AddControllers();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddSingleton(correlationOptions);
builder.Services.AddTransient<CorrelationIdMiddleware>();

builder.Services.AddApiVersioningWithExplorer();
builder.Services.AddOpenApiWithVersioning();

builder.Services.AddBasicRateLimiting(builder.Configuration);

builder.Services.AddInfrastructureHealthChecks(builder.Configuration);

// OpenTelemetry
builder.Services.AddOpenTelemetryTracing(otelOptions);

var app = builder.Build();

// ============================================================
// Middleware pipeline
// ============================================================
app.UseExceptionHandler();

app.UseSerilogRequestLogging();

app.UseRateLimiter();

app.UseMiddleware<CorrelationIdMiddleware>();

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApiEndpoints();
}

app.MapHealthCheckEndpoints();

app.Run();

public partial class Program { }
