using MediatR;

namespace Logistics.Api.BuildingBlocks.Domain.Abstractions;

/// <summary>
/// Domain Event dùng cho in-process dispatch.
/// Integration events sẽ là lớp khác (Contracts) và đi qua Outbox/RabbitMQ.
/// </summary>
public interface IDomainEvent : INotification
{
    Guid EventId { get; }
    DateTimeOffset OccurredOn { get; }
}
