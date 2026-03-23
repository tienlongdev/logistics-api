using System.Text.Json;
using Logistics.Api.BuildingBlocks.Infrastructure.Messaging;
using Logistics.Api.Shipments.Application.Abstractions;
using Logistics.Api.Shipments.Infrastructure.Persistence;

namespace Logistics.Api.Shipments.Infrastructure.Messaging;

internal sealed class OutboxMessageWriter : IOutboxMessageWriter
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly ShipmentsDbContext _context;

    public OutboxMessageWriter(ShipmentsDbContext context)
    {
        _context = context;
    }

    public void Add<TMessage>(TMessage message) where TMessage : class
    {
        _context.OutboxMessages.Add(new OutboxMessage
        {
            Id = TryResolveGuid(message, "EventId") ?? Guid.NewGuid(),
            CorrelationId = TryResolveCorrelationId(message),
            Type = typeof(TMessage).FullName ?? typeof(TMessage).Name,
            Payload = JsonSerializer.Serialize(message, SerializerOptions),
            OccurredOn = TryResolveDateTimeOffset(message, "OccurredOn") ?? DateTimeOffset.UtcNow,
            RetryCount = 0,
            Status = "Pending"
        });
    }

    private static Guid? TryResolveCorrelationId<TMessage>(TMessage message)
        => TryResolveGuid(message, "CorrelationId");

    private static Guid? TryResolveGuid<TMessage>(TMessage message, string propertyName)
    {
        var property = typeof(TMessage).GetProperty(propertyName);
        if (property?.PropertyType != typeof(Guid?))
            return null;

        return (Guid?)property.GetValue(message);
    }

    private static DateTimeOffset? TryResolveDateTimeOffset<TMessage>(TMessage message, string propertyName)
    {
        var property = typeof(TMessage).GetProperty(propertyName);
        if (property?.PropertyType != typeof(DateTimeOffset))
            return null;

        return (DateTimeOffset?)property.GetValue(message);
    }
}