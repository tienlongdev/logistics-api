namespace Logistics.Api.BuildingBlocks.Contracts;

public sealed record WebhookDeliveryRequestedPayload(
    Guid DeliveryId,
    Guid SubscriptionId,
    Guid MerchantId,
    string EventType,
    string CallbackUrl,
    DateTimeOffset RequestedAt);

public sealed record WebhookDeliveryRequestedIntegrationEvent(
    Guid EventId,
    Guid? CorrelationId,
    DateTimeOffset OccurredOn,
    int Version,
    WebhookDeliveryRequestedPayload Payload)
    : IntegrationEventEnvelope<WebhookDeliveryRequestedPayload>(EventId, CorrelationId, OccurredOn, Version, Payload);

public sealed record WebhookDeliveryRetriedPayload(
    Guid DeliveryId,
    Guid SubscriptionId,
    int AttemptCount,
    DateTimeOffset NextRetryAt,
    string? LastError);

public sealed record WebhookDeliveryRetriedIntegrationEvent(
    Guid EventId,
    Guid? CorrelationId,
    DateTimeOffset OccurredOn,
    int Version,
    WebhookDeliveryRetriedPayload Payload)
    : IntegrationEventEnvelope<WebhookDeliveryRetriedPayload>(EventId, CorrelationId, OccurredOn, Version, Payload);