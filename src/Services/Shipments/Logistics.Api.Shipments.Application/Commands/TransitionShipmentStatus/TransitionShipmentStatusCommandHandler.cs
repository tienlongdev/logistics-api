using Logistics.Api.BuildingBlocks.Application.Abstractions.CQRS;
using Logistics.Api.BuildingBlocks.Application.Results;
using Logistics.Api.BuildingBlocks.Contracts;
using Logistics.Api.BuildingBlocks.Domain.Time;
using Logistics.Api.Shipments.Application.Abstractions;
using Logistics.Api.Shipments.Application.Errors;
using Logistics.Api.Shipments.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Logistics.Api.Shipments.Application.Commands.TransitionShipmentStatus;

internal sealed class TransitionShipmentStatusCommandHandler
    : ICommandHandler<TransitionShipmentStatusCommand, Result<TransitionShipmentStatusResponse>>
{
    private readonly IShipmentRepository _shipmentRepository;
    private readonly IOutboxMessageWriter _outboxMessageWriter;
    private readonly IClock _clock;
    private readonly ILogger<TransitionShipmentStatusCommandHandler> _logger;

    public TransitionShipmentStatusCommandHandler(
        IShipmentRepository shipmentRepository,
        IOutboxMessageWriter outboxMessageWriter,
        IClock clock,
        ILogger<TransitionShipmentStatusCommandHandler> logger)
    {
        _shipmentRepository = shipmentRepository;
        _outboxMessageWriter = outboxMessageWriter;
        _clock = clock;
        _logger = logger;
    }

    public async Task<Result<TransitionShipmentStatusResponse>> Handle(
        TransitionShipmentStatusCommand command,
        CancellationToken cancellationToken)
    {
        var shipment = await _shipmentRepository.GetByIdAsync(command.ShipmentId, cancellationToken);
        if (shipment is null)
            return Result<TransitionShipmentStatusResponse>.Failure(ShipmentErrors.NotFound);

        var fromStatus = shipment.CurrentStatus;
        var succeeded = shipment.TryTransition(
            toStatus: command.ToStatus,
            note: command.Note,
            operatorId: command.OperatorId,
            operatorName: command.OperatorName,
            hubId: command.HubId,
            hubCode: command.HubCode,
            location: command.Location,
            source: "API",
            clock: _clock,
            occurredAt: command.OccurredAt);

        if (!succeeded)
            return Result<TransitionShipmentStatusResponse>.Failure(
                ShipmentErrors.InvalidStatusTransition(fromStatus, command.ToStatus));

        var trackingEvent = shipment.TrackingEvents[^1];

        _outboxMessageWriter.Add(new ShipmentStatusChangedIntegrationEvent(
            EventId: Guid.NewGuid(),
            CorrelationId: command.CorrelationId,
            OccurredOn: _clock.UtcNow,
            Version: 1,
            Payload: new ShipmentStatusChangedPayload(
                ShipmentId: shipment.Id,
                TrackingCode: shipment.TrackingCode,
                ShipmentCode: shipment.ShipmentCode,
                FromStatus: trackingEvent.FromStatus?.ToString(),
                ToStatus: trackingEvent.ToStatus.ToString(),
                HubId: trackingEvent.HubId,
                HubCode: trackingEvent.HubCode,
                Location: trackingEvent.Location,
                Note: trackingEvent.Note,
                OperatorId: trackingEvent.OperatorId,
                OperatorName: trackingEvent.OperatorName,
                OccurredAt: trackingEvent.OccurredAt)));

        await _shipmentRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Shipment {ShipmentId} transitioned from {FromStatus} to {ToStatus}",
            shipment.Id,
            fromStatus,
            shipment.CurrentStatus);

        return Result<TransitionShipmentStatusResponse>.Success(new TransitionShipmentStatusResponse(
            ShipmentId: shipment.Id,
            TrackingCode: shipment.TrackingCode,
            FromStatus: fromStatus.ToString(),
            ToStatus: shipment.CurrentStatus.ToString(),
            OccurredAt: trackingEvent.OccurredAt,
            CurrentStatus: shipment.CurrentStatus.ToString()));
    }
}