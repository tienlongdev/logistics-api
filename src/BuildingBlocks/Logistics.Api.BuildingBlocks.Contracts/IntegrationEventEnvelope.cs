namespace Logistics.Api.BuildingBlocks.Contracts;

public abstract record IntegrationEventEnvelope<TPayload>(
    Guid EventId,
    Guid? CorrelationId,
    DateTimeOffset OccurredOn,
    int Version,
    TPayload Payload);