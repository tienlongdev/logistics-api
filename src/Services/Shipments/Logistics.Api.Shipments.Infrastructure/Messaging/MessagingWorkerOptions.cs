namespace Logistics.Api.Shipments.Infrastructure.Messaging;

public sealed class MessagingWorkerOptions
{
    public const string SectionName = "Messaging:Worker";

    public int PollIntervalSeconds { get; init; } = 5;
    public int BatchSize { get; init; } = 20;
    public int MaxRetryCount { get; init; } = 10;
    public int BaseRetryDelaySeconds { get; init; } = 5;
    public int MaxRetryDelaySeconds { get; init; } = 300;
}