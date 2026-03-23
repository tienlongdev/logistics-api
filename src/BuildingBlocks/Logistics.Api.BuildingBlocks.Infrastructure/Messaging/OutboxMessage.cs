namespace Logistics.Api.BuildingBlocks.Infrastructure.Messaging;

/// <summary>
/// Outbox pattern: store integration events trong DB transaction,
/// worker sẽ publish ra message broker sau.
/// </summary>
public sealed class OutboxMessage
{
    public Guid Id { get; set; }
    public Guid? CorrelationId { get; set; }

    /// <summary>Full type name của integration event.</summary>
    public string Type { get; set; } = default!;

    /// <summary>JSON payload.</summary>
    public string Payload { get; set; } = default!;

    public DateTimeOffset OccurredOn { get; set; }
    public DateTimeOffset? NextRetryAt { get; set; }

    public DateTimeOffset? ProcessedOn { get; set; }
    public string? Error { get; set; }
    public int RetryCount { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, Processed, Failed
}
