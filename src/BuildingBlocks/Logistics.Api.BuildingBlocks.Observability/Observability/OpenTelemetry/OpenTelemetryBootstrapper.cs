using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Logistics.Api.BuildingBlocks.Observability.Observability.OpenTelemetry;

/// <summary>
/// Bootstrap cấu hình OpenTelemetry Tracing cho application.
///
/// OpenTelemetry là chuẩn để thu thập và xuất (export) dữ liệu tracing,
/// giúp theo dõi một request xuyên suốt nhiều layer và service trong hệ thống.
///
/// Class này đóng vai trò:
/// - Kích hoạt distributed tracing cho application
/// - Định danh service (service name, version)
/// - Tự động instrument các thành phần (ASP.NET Core, HttpClient)
/// - Export trace data tới hệ thống bên ngoài (Jaeger / OpenTelemetry Collector)
///
/// Flow hoạt động:
/// --------
/// HTTP Request
///   ↓
/// ASP.NET Core (tạo root span)
///   ↓
/// Application logic
///   ↓
/// HttpClient (tạo child spans)
///   ↓
/// OpenTelemetry SDK
///   ↓
/// OTLP Exporter
///   ↓
/// Jaeger / Collector
///
/// Ví dụ:
/// --------
/// Request:
///   POST /api/v1/shipments
///
/// Flow trong hệ thống:
///   Client
///     → Logistics.Api.Host
///     → Shipments
///     → Pricing (HTTP call)
///
/// Trên Jaeger:
///   Service: logistics-api
///   Trace:
///     POST /shipments (200ms)
///       → Shipments handler (50ms)
///       → Pricing HTTP call (120ms)
///
/// Từ đó có thể:
/// - xác định bottleneck
/// - biết request đi qua đâu
/// - đo latency từng bước
///
/// Các thành phần chính:
///
/// - ConfigureResource:
///     Gắn metadata cho service (ServiceName, ServiceVersion)
///     → giúp nhận diện service trong Jaeger/Grafana
///
/// - AddAspNetCoreInstrumentation:
///     Tự động tạo span cho mỗi HTTP request vào API
///
/// - AddHttpClientInstrumentation:
///     Tự động tạo span cho các outbound HTTP calls
///
/// - AddOtlpExporter:
///     Gửi trace data qua OTLP endpoint tới Jaeger hoặc OpenTelemetry Collector
///
/// Lưu ý:
/// - OpenTelemetry sử dụng TraceId/SpanId để tracing
/// - Khác với CorrelationId (dùng cho log correlation)
/// - Trong production, nên dùng song song:
///     + CorrelationId → trace business flow (logs)
///     + TraceId → trace system flow (distributed tracing)
///
/// Hạn chế hiện tại:
/// - Chưa instrument database (EF Core), Redis, RabbitMQ
/// - Sẽ được bổ sung ở các bước tiếp theo
///
/// Kết luận:
/// Đây là điểm cấu hình trung tâm để hệ thống có khả năng distributed tracing,
/// cho phép quan sát và debug request flow end-to-end trong môi trường production.
/// </summary>
public static class OpenTelemetryBootstrapper
{
    public static IServiceCollection AddOpenTelemetryTracing(
        this IServiceCollection services,
        OpenTelemetryOptions options)
    {
        services.AddOpenTelemetry()
            .ConfigureResource(resource =>
                resource.AddService(serviceName: options.ServiceName, serviceVersion: options.ServiceVersion))
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation();

                tracing.AddOtlpExporter(exporter =>
                {
                    exporter.Endpoint = new Uri(options.OtlpEndpoint);
                });
            });

        return services;
    }
}
