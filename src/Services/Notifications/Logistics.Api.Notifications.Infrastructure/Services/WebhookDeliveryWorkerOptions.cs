namespace Logistics.Api.Notifications.Infrastructure.Services;

public sealed class WebhookDeliveryWorkerOptions
{
    public const string SectionName = "Notifications:WebhookDelivery";

    public int PollIntervalSeconds { get; init; } = 5;
    public int BatchSize { get; init; } = 20;
    public int MaxAttempts { get; init; } = 8;
    public int BaseRetryDelaySeconds { get; init; } = 10;
    public int MaxRetryDelaySeconds { get; init; } = 600;
    public int MaxLoggedResponseBodyLength { get; init; } = 2000;
}