using FluentValidation;

namespace Logistics.Api.Shipments.Application.Commands.TransitionShipmentStatus;

public sealed class TransitionShipmentStatusCommandValidator : AbstractValidator<TransitionShipmentStatusCommand>
{
    public TransitionShipmentStatusCommandValidator()
    {
        RuleFor(x => x.ShipmentId).NotEmpty();
        RuleFor(x => x.HubCode).MaximumLength(20);
        RuleFor(x => x.Location).MaximumLength(255);
        RuleFor(x => x.OperatorName).MaximumLength(255);
    }
}