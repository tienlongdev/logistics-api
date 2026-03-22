using System.Threading.RateLimiting;

namespace Logistics.Api.Host.Extensions;

/// <summary>
/// Extension đăng ký Rate Limiting cơ bản cho toàn bộ application.
///
/// Rate limiting được sử dụng để kiểm soát số lượng request mà client có thể gửi
/// trong một khoảng thời gian nhất định, nhằm:
/// - Bảo vệ hệ thống khỏi abuse / DDoS
/// - Ngăn client gửi quá nhiều request trong thời gian ngắn
/// - Đảm bảo fairness giữa các client
///
/// Class này cấu hình:
/// - Global rate limiter (áp dụng cho toàn bộ request)
/// - Chiến lược: Fixed Window Rate Limiting
/// - Partition key: theo IP address (MVP level)
///
/// Cách hoạt động:
/// --------
/// Với mỗi request:
/// - Lấy IP từ HttpContext
/// - Map IP → một rate limiter riêng (partition)
/// - Kiểm tra số request trong window hiện tại
///
/// Nếu vượt limit:
/// - Request bị reject với HTTP 429 (Too Many Requests)
///
/// Ví dụ:
/// --------
/// Cấu hình:
///   PermitLimit = 100
///   Window = 60 seconds
///
/// Nghĩa là:
///   Mỗi IP chỉ được gửi tối đa 100 request / 60 giây
///
/// Scenario:
///   Client A (IP: 1.1.1.1)
///     → gửi 100 request → OK
///     → request thứ 101 → bị reject (429)
///
/// Response:
///   HTTP 429 Too Many Requests
///
/// Thiết kế:
/// --------
/// - GlobalLimiter:
///     Áp dụng cho toàn bộ pipeline (tất cả endpoint)
///
/// - PartitionedRateLimiter:
///     Cho phép mỗi "key" (ở đây là IP) có limiter riêng
///
/// - FixedWindowRateLimiter:
///     Chia thời gian thành các window cố định (ví dụ 60s)
///     → đơn giản, dễ predict, phù hợp cho MVP
///
/// - QueueLimit = 0:
///     Không xếp hàng request → reject ngay nếu vượt limit
///
/// - QueueProcessingOrder = OldestFirst:
///     (không có effect khi QueueLimit = 0, nhưng chuẩn bị cho future)
///
/// Lưu ý:
/// --------
/// - Hiện tại partition theo IP:
///     + Đơn giản, phù hợp giai đoạn đầu
///     + Không chính xác trong môi trường:
///         - NAT
///         - Proxy / Load balancer
///
/// - Trong production nâng cao:
///     + Có thể partition theo:
///         - UserId (authenticated)
///         - MerchantId
///         - API key
///
/// - Fixed Window có nhược điểm:
///     + Có thể burst ở boundary của window
///     → có thể thay bằng Sliding Window / Token Bucket ở step sau
///
/// - Rate limiting này chạy in-memory:
///     + Không shared giữa nhiều instance
///     + Khi scale nhiều node → cần distributed rate limiting (Redis)
///
/// Configuration:
/// --------
/// Có thể override qua appsettings.json:
///
/// {
///   "RateLimiting": {
///     "GlobalPermitLimit": 100,
///     "GlobalWindowSeconds": 60
///   }
/// }
///
/// Kết luận:
/// --------
/// Đây là lớp cấu hình rate limiting ở mức cơ bản (MVP),
/// giúp bảo vệ hệ thống khỏi quá tải và abuse,
/// đồng thời đặt nền tảng để mở rộng lên các chiến lược rate limiting phức tạp hơn trong tương lai.
/// </summary>
public static class RateLimitingExtensions
{
    public static IServiceCollection AddBasicRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        var permitLimit = configuration.GetValue<int?>("RateLimiting:GlobalPermitLimit") ?? 100;
        var windowSeconds = configuration.GetValue<int?>("RateLimiting:GlobalWindowSeconds") ?? 60;

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            // Global limiter
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
            {
                // Key theo IP (đủ cho MVP). Step sau có thể theo user/merchant.
                var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: ip,
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = permitLimit,
                        Window = TimeSpan.FromSeconds(windowSeconds),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    });
            });
        });

        return services;
    }
}
