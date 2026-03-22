using Microsoft.AspNetCore.Http;
using Serilog.Context;

namespace Logistics.Api.BuildingBlocks.Observability.Observability.Correlation;

/// <summary>
/// Middleware đảm bảo mỗi HTTP request đều có một Correlation ID nhất quán trong suốt vòng đời xử lý.
///
/// Correlation ID là định danh duy nhất đại diện cho một request hoặc business flow,
/// được dùng để trace và liên kết log xuyên suốt các layer (API → Service → DB → Message Bus → Worker).
///
/// Hành vi của middleware:
/// 1. Kiểm tra request header (mặc định: "X-Correlation-Id"):
///    - Nếu client/upstream service đã gửi Correlation ID → sử dụng lại (preserve flow).
///    - Nếu không có → generate một Correlation ID mới.
///
/// 2. Gắn Correlation ID vào:
///    - HttpContext.TraceIdentifier → đồng bộ với request identifier của ASP.NET Core
///    - Response header → để client/debugger có thể lấy lại ID
///    - Serilog LogContext → mọi log trong request scope đều chứa CorrelationId
///
/// 3. Đảm bảo Correlation ID tồn tại xuyên suốt pipeline:
///    - Từ middleware → controller → service → repository → outbound HTTP → message bus
///
/// Mục tiêu:
/// - Trace end-to-end một request trong hệ thống phân tán
/// - Liên kết log giữa nhiều module/service
/// - Hỗ trợ debugging, monitoring và incident investigation trong production
///
/// Ví dụ:
/// --------
/// Client gửi request:
///   POST /api/v1/shipments
///   Header: X-Correlation-Id: abc123
///
/// Flow:
///   Client
///     → Logistics.Api.Host
///     → Shipments module
///     → Pricing module
///     → PostgreSQL
///     → Publish event (RabbitMQ)
///     → Tracking consumer
///     → Notifications consumer
///
/// Tất cả log trong flow sẽ có:
///   CorrelationId = abc123
///
/// Ví dụ log (Serilog):
///   [INF] Creating shipment {ShipmentId} CorrelationId=abc123
///   [INF] Calculating price CorrelationId=abc123
///   [INF] Publishing ShipmentCreated event CorrelationId=abc123
///
/// Điều này cho phép tìm toàn bộ log liên quan bằng một query duy nhất.
///
/// Nếu client KHÔNG gửi header:
///   → Middleware sẽ generate:
///     X-Correlation-Id: d4c31f1a8f9647f8b8c2d75d46d0d111
///
/// Và response trả về:
///   Header: X-Correlation-Id: d4c31f1a8f9647f8b8c2d75d46d0d111
///
/// Lưu ý quan trọng:
/// - Correlation ID ≠ TraceId (OpenTelemetry) ≠ SpanId
///   + Correlation ID: phục vụ business-level tracing (log correlation)
///   + TraceId/SpanId: phục vụ distributed tracing (Jaeger/Grafana)
///
/// - Khi gọi downstream services hoặc publish message (RabbitMQ),
///   Correlation ID này nên được propagate tiếp để đảm bảo trace xuyên suốt hệ thống.
///
/// - Việc override HttpContext.TraceIdentifier là một quyết định thiết kế:
///   giúp đồng bộ ID trong pipeline, nhưng sẽ thay thế request ID mặc định của ASP.NET Core.
///
/// Kết luận:
/// Middleware này là nền tảng cho observability, đảm bảo mỗi request đều có thể
/// được trace xuyên suốt toàn hệ thống thông qua một Correlation ID duy nhất.
/// </summary>
public sealed class CorrelationIdMiddleware : IMiddleware
{
    private readonly CorrelationIdOptions _options;

    public CorrelationIdMiddleware(CorrelationIdOptions options)
    {
        _options = options;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var headerName = _options.HeaderName;

        var correlationId = context.Request.Headers.TryGetValue(headerName, out var values) && !string.IsNullOrWhiteSpace(values.ToString())
            ? values.ToString()
            : Guid.NewGuid().ToString("N");

        context.Response.OnStarting(() =>
        {
            context.Response.Headers[headerName] = correlationId;
            return Task.CompletedTask;
        });

        // Gắn vào trace identifier để có thể map với logs/traces
        context.TraceIdentifier = correlationId;

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await next(context);
        }
    }
}
