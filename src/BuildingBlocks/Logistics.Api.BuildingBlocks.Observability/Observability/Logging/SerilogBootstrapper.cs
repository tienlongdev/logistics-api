using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;

namespace Logistics.Api.BuildingBlocks.Observability.Observability.Logging;

/// <summary>
/// Bootstrap cấu hình Serilog cho toàn bộ application.
///
/// Vai trò:
/// - Khởi tạo logger configuration từ appsettings.json
/// - Thiết lập mức log mặc định và override cho framework (Microsoft/System)
/// - Enrich log với metadata phục vụ observability và debugging
///
/// Các enrichment được thêm vào:
/// - Application: tên service (giúp phân biệt khi chạy multi-service)
/// - Environment: môi trường chạy (Development/Staging/Production)
/// - MachineName: tên máy/container
/// - ProcessId: ID process
/// - ThreadId: ID thread (phục vụ debug low-level)
/// - LogContext: cho phép inject dynamic properties (ví dụ CorrelationId)
///
/// Lưu ý:
/// - CorrelationId được push vào LogContext từ middleware và sẽ xuất hiện trong mọi log
/// - Cấu hình sink (Console, Seq, Elasticsearch...) được đọc từ appsettings.json
/// - Override log level cho Microsoft/System để giảm noise từ framework
///
/// Ví dụ:
/// Log sau khi enrich:
/// {
///   "Message": "Creating shipment",
///   "CorrelationId": "abc123",
///   "Application": "Logistics.Api.Host",
///   "Environment": "Development"
/// }
/// </summary>
public static class SerilogBootstrapper
{
    public static LoggerConfiguration CreateLoggerConfiguration(IConfiguration configuration, string applicationName)
    {
        return new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", applicationName)
            .Enrich.WithEnvironmentName()
            .Enrich.WithMachineName()
            .Enrich.WithProcessId()
            .Enrich.WithThreadId();
    }
}
