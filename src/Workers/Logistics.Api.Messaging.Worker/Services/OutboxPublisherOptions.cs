namespace Logistics.Api.Messaging.Worker.Services;

public sealed class OutboxPublisherOptions
{
    public const string SectionName = "Messaging:OutboxPublisher";

    public int PollIntervalSeconds { get; init; } = 5;
    public int BatchSize { get; init; } = 50;
    public int MaxRetryCount { get; init; } = 10;
    public int BaseRetryDelaySeconds { get; init; } = 5;
    public int MaxRetryDelaySeconds { get; init; } = 300;
}