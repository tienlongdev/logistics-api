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
using Logistics.Api.Merchants.Infrastructure;
using Logistics.Api.Notifications.Infrastructure;
using Logistics.Api.Pricing.Infrastructure;
using Logistics.Api.Search.Infrastructure;
using Logistics.Api.Shipments.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

const string CorsPolicyName = "WebClientCors";

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
var correlationOptions = builder.Configuration
    .GetSection(CorrelationIdOptions.SectionName)
    .Get<CorrelationIdOptions>() ?? new CorrelationIdOptions();

var otelOptions = builder.Configuration
    .GetSection(OpenTelemetryOptions.SectionName)
    .Get<OpenTelemetryOptions>() ?? new OpenTelemetryOptions();

var jwtOptions = builder.Configuration
    .GetSection(JwtOptions.SectionName)
    .Get<JwtOptions>() ?? throw new InvalidOperationException("Missing Jwt configuration section.");

// ============================================================
// Services
// ============================================================
builder.Services.AddControllers();

var corsOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>()
    ?? ["http://localhost:3000", "http://localhost:3001", "http://127.0.0.1:3000", "http://127.0.0.1:3001"];

builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicyName, policy =>
    {
        policy
            .WithOrigins(corsOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddSingleton(correlationOptions);
builder.Services.AddTransient<CorrelationIdMiddleware>();

builder.Services.AddApiVersioningWithExplorer();

// OpenAPI / Swagger
builder.Services.AddOpenApiWithVersioning();

// Nếu extension hiện tại chưa đủ, bật thêm 2 dòng dưới:
// builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen();

builder.Services.AddBasicRateLimiting(builder.Configuration);
builder.Services.AddInfrastructureHealthChecks(builder.Configuration);

// OpenTelemetry
builder.Services.AddOpenTelemetryTracing(otelOptions);

// Redis distributed cache
var redisConnectionString = builder.Configuration.GetConnectionString("Redis");
if (!string.IsNullOrWhiteSpace(redisConnectionString))
{
    builder.Services.AddStackExchangeRedisCache(opts =>
    {
        opts.Configuration = redisConnectionString;
        opts.InstanceName = "logistics:";
    });
}
else
{
    builder.Services.AddDistributedMemoryCache();
}

// Modules
builder.Services.AddIdentityModule(builder.Configuration);
builder.Services.AddMerchantsModule(builder.Configuration);
builder.Services.AddNotificationsModule(builder.Configuration);
builder.Services.AddPricingModule(builder.Configuration);
builder.Services.AddSearchModule(builder.Configuration);
builder.Services.AddShipmentsModule(builder.Configuration);

// MediatR pipeline behaviors
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationPipelineBehavior<,>));

// JWT Authentication
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

// RBAC Authorization Policies
builder.Services.AddAuthorization(opts =>
{
    opts.AddPolicy("AdminOnly", p => p.RequireRole(Role.Names.Admin));
    opts.AddPolicy("OperatorOrAdmin", p => p.RequireRole(Role.Names.Admin, Role.Names.Operator));
    opts.AddPolicy("HubStaffOrAbove", p => p.RequireRole(Role.Names.Admin, Role.Names.Operator, Role.Names.HubStaff));
    opts.AddPolicy("MerchantOnly", p => p.RequireRole(Role.Names.Merchant));
    opts.AddPolicy("MerchantOrAdmin", p => p.RequireRole(Role.Names.Admin, Role.Names.Merchant));
});

var app = builder.Build();

await app.Services.MigrateAndSeedIdentityAsync(app.Configuration);

// ============================================================
// Middleware pipeline
// ============================================================
app.UseExceptionHandler();

app.UseSerilogRequestLogging();

app.UseRateLimiter();

app.UseMiddleware<CorrelationIdMiddleware>();

app.UseCors(CorsPolicyName);

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// Log environment để debug
Log.Information("Environment: {Environment}", app.Environment.EnvironmentName);

// Map controllers
app.MapControllers();

// Map OpenAPI / Swagger cho mọi environment local/container
app.MapOpenApiEndpoints();

// Nếu extension của bạn không map UI thật, dùng thêm:
// app.UseSwagger();
// app.UseSwaggerUI();

app.MapHealthCheckEndpoints();

app.Run();

public partial class Program { }