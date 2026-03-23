using System.Text;
using Logistics.Api.BuildingBlocks.Application.Behaviors;
using Logistics.Api.BuildingBlocks.Observability.Observability.Correlation;
using Logistics.Api.BuildingBlocks.Observability.Observability.Logging;
using Logistics.Api.BuildingBlocks.Observability.Observability.OpenTelemetry;
using Logistics.Api.Host.Extensions;
using Logistics.Api.Host.Middleware;
using Logistics.Api.Identity.Domain.Entities;
using Logistics.Api.Identity.Infrastructure;
using Logistics.Api.Identity.Infrastructure.Services;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
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

var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
    ?? throw new InvalidOperationException("Missing Jwt configuration section.");

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

// ── Modules ──────────────────────────────────────────────────────────────────
builder.Services.AddIdentityModule(builder.Configuration);

// ── MediatR pipeline behaviors (global, registered after all module handlers) ─
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationPipelineBehavior<,>));

// ── JWT Authentication ────────────────────────────────────────────────────────
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opts =>
    {
        opts.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

// ── RBAC Authorization Policies (B2) ─────────────────────────────────────────
builder.Services.AddAuthorization(opts =>
{
    opts.AddPolicy("AdminOnly", p => p.RequireRole(Role.Names.Admin));
    opts.AddPolicy("OperatorOrAdmin", p => p.RequireRole(Role.Names.Admin, Role.Names.Operator));
    opts.AddPolicy("HubStaffOrAbove", p => p.RequireRole(Role.Names.Admin, Role.Names.Operator, Role.Names.HubStaff));
    opts.AddPolicy("MerchantOnly", p => p.RequireRole(Role.Names.Merchant));
    opts.AddPolicy("MerchantOrAdmin", p => p.RequireRole(Role.Names.Admin, Role.Names.Merchant));
});

var app = builder.Build();

// ============================================================
// Middleware pipeline
// ============================================================
app.UseExceptionHandler();

app.UseSerilogRequestLogging();

app.UseRateLimiter();

app.UseMiddleware<CorrelationIdMiddleware>();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApiEndpoints();
}

app.MapHealthCheckEndpoints();

app.Run();

public partial class Program { }

