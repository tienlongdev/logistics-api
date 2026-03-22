using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using RabbitMQ.Client;

namespace Logistics.Api.Host.Extensions;

/// <summary>
/// Extension đăng ký và map các Health Check endpoint cho application.
///
/// Mục tiêu:
/// - Kiểm tra tình trạng sống (liveness) của process
/// - Kiểm tra mức sẵn sàng (readiness) của application
/// - Xác minh khả năng kết nối tới các hạ tầng phụ thuộc:
///   PostgreSQL, Redis, RabbitMQ, Elasticsearch
///
/// Health Checks giúp hệ thống:
/// - Phục vụ monitoring
/// - Tích hợp với container orchestrators (Docker, Kubernetes)
/// - Hỗ trợ vận hành production an toàn hơn
///
/// Các loại endpoint:
///
/// - /health
///   Endpoint tổng quát, trả về trạng thái health của application và các dependency
///
/// - /health/live
///   Liveness probe
///   Dùng để xác định process còn sống hay không
///   Không nên phụ thuộc vào DB / Redis / RabbitMQ
///
/// - /health/ready
///   Readiness probe
///   Dùng để xác định application đã sẵn sàng nhận traffic hay chưa
///   Nên bao gồm các dependency cần thiết để phục vụ request
///
/// Ví dụ:
/// --------
/// Nếu PostgreSQL down:
/// - /health/live  → vẫn có thể Healthy
/// - /health/ready → Unhealthy
///
/// Điều này cho phép orchestrator biết:
/// - app process vẫn đang chạy
/// - nhưng chưa sẵn sàng phục vụ request
///
/// Response sử dụng <see cref="UIResponseWriter.WriteHealthCheckUIResponse"/>
/// để trả JSON theo format thân thiện cho UI/monitoring tools.
///
/// Lưu ý:
/// - Các connection string được đọc từ configuration
/// - Nếu một connection string không được cấu hình, health check tương ứng sẽ không được đăng ký
/// - Đây là foundation cho production readiness của hệ thống
/// </summary>
public static class HealthChecksExtensions
{
    public static IServiceCollection AddInfrastructureHealthChecks(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var postgres = configuration.GetConnectionString("Postgres");
        var redis = configuration.GetConnectionString("Redis");
        var rabbit = configuration.GetConnectionString("RabbitMQ");
        var elastic = configuration.GetConnectionString("Elasticsearch");

        var hc = services.AddHealthChecks();

        if (!string.IsNullOrWhiteSpace(postgres))
            hc.AddNpgSql(postgres, name: "postgres");

        if (!string.IsNullOrWhiteSpace(redis))
            hc.AddRedis(redis, name: "redis");

        if (!string.IsNullOrWhiteSpace(rabbit))
        {
            hc.AddRabbitMQ(
                async _ =>
                {
                    var factory = new ConnectionFactory
                    {
                        Uri = new Uri(rabbit)
                    };

                    return await factory.CreateConnectionAsync();
                },
                name: "rabbitmq");
        }

        if (!string.IsNullOrWhiteSpace(elastic))
            hc.AddElasticsearch(elastic, name: "elasticsearch");

        return services;
    }

    public static WebApplication MapHealthCheckEndpoints(this WebApplication app)
    {
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = _ => false,
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        return app;
    }
}
