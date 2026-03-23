namespace Logistics.Api.BuildingBlocks.Contracts;

public sealed record ShipmentCreatedPayload(
    Guid ShipmentId,
    string TrackingCode,
    string ShipmentCode,
    Guid MerchantId,
    string MerchantCode,
    string CurrentStatus,
    string ServiceType,
    decimal ShippingFee,
    decimal CodAmount,
    decimal TotalFee,
    DateTimeOffset CreatedAt);

public sealed record ShipmentCreatedIntegrationEvent(
    Guid EventId,
    Guid? CorrelationId,
    DateTimeOffset OccurredOn,
    int Version,
    ShipmentCreatedPayload Payload)
    : IntegrationEventEnvelope<ShipmentCreatedPayload>(EventId, CorrelationId, OccurredOn, Version, Payload);

public sealed record ShipmentStatusChangedPayload(
    Guid ShipmentId,
    string TrackingCode,
    string ShipmentCode,
    string? FromStatus,
    string ToStatus,
    Guid? HubId,
    string? HubCode,
    string? Location,
    string? Note,
    Guid? OperatorId,
    string? OperatorName,
    DateTimeOffset OccurredAt);

public sealed record ShipmentStatusChangedIntegrationEvent(
    Guid EventId,
    Guid? CorrelationId,
    DateTimeOffset OccurredOn,
    int Version,
    ShipmentStatusChangedPayload Payload)
    : IntegrationEventEnvelope<ShipmentStatusChangedPayload>(EventId, CorrelationId, OccurredOn, Version, Payload);

public sealed record ShipmentAssignedToHubPayload(
    Guid ShipmentId,
    string TrackingCode,
    Guid HubId,
    string HubCode,
    DateTimeOffset AssignedAt);

public sealed record ShipmentAssignedToHubIntegrationEvent(
    Guid EventId,
    Guid? CorrelationId,
    DateTimeOffset OccurredOn,
    int Version,
    ShipmentAssignedToHubPayload Payload)
    : IntegrationEventEnvelope<ShipmentAssignedToHubPayload>(EventId, CorrelationId, OccurredOn, Version, Payload);