namespace Logistics.Api.BuildingBlocks.Observability.Observability.Correlation;

/// <summary>
/// Cấu hình cho Correlation ID trong toàn hệ thống.
///
/// Correlation ID là định danh duy nhất đại diện cho một request / business flow,
/// được propagate xuyên suốt các layer và service (HTTP → DB → Message Bus → background workers).
///
/// Mục đích:
/// - Cho phép trace một flow end-to-end trong hệ thống phân tán
/// - Liên kết log giữa nhiều module/service khác nhau
/// - Hỗ trợ debugging, monitoring và incident investigation trong production
///
/// Convention:
/// - Correlation ID được truyền qua HTTP header (mặc định: "X-Correlation-Id")
/// - Nếu client không cung cấp, hệ thống sẽ tự generate và gắn vào request/response
/// - Header này phải được propagate khi gọi downstream services hoặc publish integration events
///
/// Lưu ý:
/// - Correlation ID khác với TraceId (OpenTelemetry) và SpanId
/// - TraceId phục vụ distributed tracing, còn Correlation ID phục vụ business-level tracing/log correlation
/// - Tất cả services trong hệ thống phải thống nhất cùng HeaderName để đảm bảo trace xuyên suốt
///
/// Có thể override giá trị này qua cấu hình (appsettings.json):
/// {
///   "Correlation": {
///     "HeaderName": "X-Correlation-Id"
///   }
/// }
/// </summary>
public sealed class CorrelationIdOptions
{
    /// <summary>
    /// Tên section trong configuration (appsettings).
    /// </summary>
    public const string SectionName = "Correlation";

    /// <summary>
    /// Tên HTTP header dùng để truyền Correlation ID giữa client và server,
    /// cũng như giữa các service trong hệ thống.
    ///
    /// Mặc định: "X-Correlation-Id"
    /// </summary>
    public string HeaderName { get; init; } = "X-Correlation-Id";
}
