/// <summary>
/// Cấu hình cho OpenTelemetry (distributed tracing) của application.
///
/// OpenTelemetry được sử dụng để thu thập và export trace data, cho phép theo dõi
/// một request xuyên suốt nhiều layer và service (API → Application → DB → external services).
///
/// Class này định nghĩa:
/// - Danh tính của service (ServiceName, ServiceVersion)
/// - Endpoint để export trace (OTLP endpoint)
///
/// Mục tiêu:
/// - Cho phép quan sát (observability) hệ thống ở mức distributed
/// - Trace một request end-to-end trong môi trường production
/// - Phát hiện bottleneck, latency và lỗi trong flow nghiệp vụ
///
/// Ví dụ:
/// --------
/// Một request tạo shipment:
///
///   Client → Logistics.Api.Host → Shipments → Pricing → PostgreSQL
///
/// Khi OpenTelemetry được cấu hình:
/// - Mỗi bước sẽ tạo một span
/// - Toàn bộ flow sẽ được gom thành một trace
///
/// Trên Jaeger bạn sẽ thấy:
///   Service: logistics-api
///   Trace:
///     POST /shipments
///       → Shipments handler (50ms)
///       → Pricing call (120ms)
///       → DB query (300ms)
///
/// Từ đó có thể xác định chính xác bottleneck (ví dụ: DB chậm).
///
/// Các thuộc tính:
/// - <see cref="ServiceName"/>:
///     Tên service hiển thị trong tracing system (Jaeger/Grafana).
///     Dùng để phân biệt giữa các service trong hệ thống.
///     Ví dụ: "logistics-api", "shipments-service"
///
/// - <see cref="ServiceVersion"/>:
///     Version của service, phục vụ tracking deploy và debugging theo version.
///
/// - <see cref="OtlpEndpoint"/>:
///     Endpoint của OpenTelemetry Collector (OTLP),
///     nơi application gửi trace data.
///     Ví dụ: "http://localhost:4317"
///
/// Lưu ý:
/// - OpenTelemetry sử dụng TraceId/SpanId để trace hệ thống,
///   khác với CorrelationId (dùng cho log correlation).
/// - Trong hệ thống production, CorrelationId và TraceId thường được dùng song song:
///     + CorrelationId → business-level tracing (logs)
///     + TraceId → system-level tracing (distributed tracing)
///
/// - Giá trị trong class này thường được bind từ configuration (appsettings.json):
///
///   {
///     "OpenTelemetry": {
///       "ServiceName": "logistics-api",
///       "ServiceVersion": "1.0.0",
///       "OtlpEndpoint": "http://localhost:4317"
///     }
///   }
///
/// Kết luận:
/// Đây là cấu hình nền tảng để hệ thống có thể tham gia vào distributed tracing,
/// giúp quan sát và debug toàn bộ request flow trong môi trường production.
/// </summary>
public sealed class OpenTelemetryOptions
{
    /// <summary>
    /// Tên section trong configuration (appsettings).
    /// </summary>
    public const string SectionName = "OpenTelemetry";

    /// <summary>
    /// Tên service hiển thị trong tracing system (Jaeger, Grafana, v.v.).
    /// Dùng để định danh service trong hệ thống phân tán.
    /// </summary>
    public string ServiceName { get; init; } = "logistics-api";

    /// <summary>
    /// Version của service, phục vụ tracking deploy và phân tích lỗi theo version.
    /// </summary>
    public string ServiceVersion { get; init; } = "1.0.0";

    /// <summary>
    /// OTLP endpoint (OpenTelemetry Protocol) để export trace data.
    /// Ví dụ: http://localhost:4317 (OpenTelemetry Collector / Jaeger).
    /// </summary>
    public string OtlpEndpoint { get; init; } = "http://localhost:4317";
}
