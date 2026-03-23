using Logistics.Api.BuildingBlocks.Application.Abstractions.CQRS;
using Logistics.Api.BuildingBlocks.Application.Results;
using Logistics.Api.Shipments.Domain.Enums;

namespace Logistics.Api.Shipments.Application.Commands.TransitionShipmentStatus;

public sealed record TransitionShipmentStatusCommand(
    Guid ShipmentId,
    ShipmentStatus ToStatus,
    string? Note,
    Guid? HubId,
    string? HubCode,
    string? Location,
    DateTimeOffset? OccurredAt,
    Guid? OperatorId,
    string? OperatorName,
    Guid? CorrelationId) : ICommand<Result<TransitionShipmentStatusResponse>>;