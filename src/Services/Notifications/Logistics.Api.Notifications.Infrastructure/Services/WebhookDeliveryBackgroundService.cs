using System.Security.Cryptography;
using System.Text;
using Logistics.Api.Notifications.Infrastructure.Persistence;
using Logistics.Api.Notifications.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Logistics.Api.Notifications.Infrastructure.Services;

public sealed class WebhookDeliveryBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly WebhookDeliveryWorkerOptions _options;
    private readonly ILogger<WebhookDeliveryBackgroundService> _logger;

    public WebhookDeliveryBackgroundService(
        IServiceScopeFactory scopeFactory,
        IHttpClientFactory httpClientFactory,
        IOptions<WebhookDeliveryWorkerOptions> options,
        ILogger<WebhookDeliveryBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var count = await ProcessDueDeliveriesAsync(stoppingToken);
                if (count == 0)
                    await Task.Delay(TimeSpan.FromSeconds(_options.PollIntervalSeconds), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Webhook delivery loop failed");
                await Task.Delay(TimeSpan.FromSeconds(_options.PollIntervalSeconds), stoppingToken);
            }
        }
    }

    private async Task<int> ProcessDueDeliveriesAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<NotificationsDbContext>();
        var now = DateTimeOffset.UtcNow;

        var deliveries = await dbContext.WebhookDeliveries
            .Include(x => x.Subscription)
            .Where(x => (x.Status == "Pending" || x.Status == "Failed") && (!x.NextRetryAt.HasValue || x.NextRetryAt <= now))
            .OrderBy(x => x.CreatedAt)
            .Take(_options.BatchSize)
            .ToListAsync(cancellationToken);

        foreach (var delivery in deliveries)
            await ProcessDeliveryAsync(dbContext, delivery, cancellationToken);

        if (deliveries.Count > 0)
            await dbContext.SaveChangesAsync(cancellationToken);

        return deliveries.Count;
    }

    private async Task ProcessDeliveryAsync(
        NotificationsDbContext dbContext,
        WebhookDeliveryEntity delivery,
        CancellationToken cancellationToken)
    {
        if (!delivery.Subscription.IsActive)
        {
            delivery.Status = "Exhausted";
            delivery.LastError = "Subscription is inactive.";
            delivery.UpdatedAt = DateTimeOffset.UtcNow;
            delivery.NextRetryAt = null;
            return;
        }

        var request = new HttpRequestMessage(HttpMethod.Post, delivery.Subscription.CallbackUrl)
        {
            Content = new StringContent(delivery.Payload, Encoding.UTF8, "application/json")
        };

        request.Headers.TryAddWithoutValidation("X-Webhook-Signature", ComputeSignature(delivery.Subscription.Secret, delivery.Payload));
        request.Headers.TryAddWithoutValidation("X-Webhook-Event", delivery.EventType);
        request.Headers.TryAddWithoutValidation("X-Webhook-Event-Id", delivery.EventId.ToString());

        var client = _httpClientFactory.CreateClient("webhook-delivery");
        HttpResponseMessage? response = null;
        string? responseBody = null;

        try
        {
            response = await client.SendAsync(request, cancellationToken);
            responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            delivery.AttemptCount += 1;
            delivery.LastResponseCode = (int)response.StatusCode;
            delivery.LastResponseBody = Truncate(responseBody);
            delivery.UpdatedAt = DateTimeOffset.UtcNow;

            if (response.IsSuccessStatusCode)
            {
                delivery.Status = "Success";
                delivery.DeliveredAt = DateTimeOffset.UtcNow;
                delivery.LastError = null;
                delivery.NextRetryAt = null;

                _logger.LogInformation(
                    "Webhook delivery {DeliveryId} succeeded with status {StatusCode}",
                    delivery.Id,
                    (int)response.StatusCode);
                return;
            }

            HandleFailure(delivery, $"HTTP {(int)response.StatusCode}");
            _logger.LogWarning(
                "Webhook delivery {DeliveryId} failed with status {StatusCode}: {ResponseBody}",
                delivery.Id,
                (int)response.StatusCode,
                Truncate(responseBody));
        }
        catch (Exception ex)
        {
            delivery.AttemptCount += 1;
            delivery.LastError = ex.Message;
            delivery.UpdatedAt = DateTimeOffset.UtcNow;
            HandleFailure(delivery, ex.Message);

            _logger.LogError(ex,
                "Webhook delivery {DeliveryId} failed on attempt {AttemptCount}",
                delivery.Id,
                delivery.AttemptCount);
        }
    }

    private void HandleFailure(WebhookDeliveryEntity delivery, string error)
    {
        delivery.LastError = error;
        delivery.DeliveredAt = null;

        if (delivery.AttemptCount >= delivery.MaxAttempts)
        {
            delivery.Status = "Exhausted";
            delivery.NextRetryAt = null;
        }
        else
        {
            delivery.Status = "Failed";
            delivery.NextRetryAt = DateTimeOffset.UtcNow.Add(CalculateBackoff(delivery.AttemptCount));
        }
    }

    private TimeSpan CalculateBackoff(int attemptCount)
    {
        var exponentialSeconds = _options.BaseRetryDelaySeconds * Math.Pow(2, Math.Max(0, attemptCount - 1));
        return TimeSpan.FromSeconds(Math.Min(exponentialSeconds, _options.MaxRetryDelaySeconds));
    }

    private string ComputeSignature(string secret, string payload)
    {
        var key = Encoding.UTF8.GetBytes(secret);
        var data = Encoding.UTF8.GetBytes(payload);
        using var hmac = new HMACSHA256(key);
        var signature = Convert.ToHexString(hmac.ComputeHash(data)).ToLowerInvariant();
        return $"sha256={signature}";
    }

    private string? Truncate(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        return value.Length <= _options.MaxLoggedResponseBodyLength
            ? value
            : value[.._options.MaxLoggedResponseBodyLength];
    }
}