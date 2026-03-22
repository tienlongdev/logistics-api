namespace Logistics.Api.BuildingBlocks.Infrastructure.Messaging;

/// <summary>
/// Inbox pattern: lưu message đã xử lý để consumer idempotent.
/// MessageId từ broker nên map vào Id.
/// </summary>
public sealed class InboxMessage
{
    public Guid Id { get; set; } // message id
    public string ConsumerName { get; set; } = default!;
    public string Type { get; set; } = default!;
    public string Payload { get; set; } = default!;

    public DateTimeOffset ReceivedOn { get; set; }
    public DateTimeOffset? ProcessedOn { get; set; }
    public string? Error { get; set; }
}
