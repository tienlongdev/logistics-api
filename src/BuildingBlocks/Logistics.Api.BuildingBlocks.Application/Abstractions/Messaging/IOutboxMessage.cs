namespace Logistics.Api.BuildingBlocks.Application.Abstractions.Messaging;

/// <summary>
/// Marker interface cho outbox messages.
/// Thực thể persistence sẽ nằm ở Infrastructure.
/// </summary>
public interface IOutboxMessage
{
    Guid Id { get; }
    Guid? CorrelationId { get; }
    string Type { get; }
    string Payload { get; }
    DateTimeOffset OccurredOn { get; }
}
