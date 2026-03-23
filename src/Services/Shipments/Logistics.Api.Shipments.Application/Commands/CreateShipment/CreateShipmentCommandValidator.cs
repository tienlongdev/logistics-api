using FluentValidation;

namespace Logistics.Api.Shipments.Application.Commands.CreateShipment;

public sealed class CreateShipmentCommandValidator : AbstractValidator<CreateShipmentCommand>
{
    public CreateShipmentCommandValidator()
    {
        RuleFor(c => c.IdempotencyKey)
            .NotEmpty()
            .MaximumLength(255)
            .WithMessage("Idempotency-Key header là bắt buộc.");

        RuleFor(c => c.Sender).NotNull().SetValidator(new AddressDtoValidator("Sender"));
        RuleFor(c => c.Receiver).NotNull().SetValidator(new AddressDtoValidator("Receiver"));

        RuleFor(c => c.Package).NotNull();
        RuleFor(c => c.Package.WeightGram)
            .GreaterThan(0)
            .WithMessage("WeightGram phải lớn hơn 0.");

        RuleFor(c => c.CodAmount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("CodAmount không được âm.");

        RuleFor(c => c.DeclaredValue)
            .GreaterThanOrEqualTo(0)
            .WithMessage("DeclaredValue không được âm.");
    }
}

internal sealed class AddressDtoValidator : AbstractValidator<AddressDto>
{
    public AddressDtoValidator(string prefix)
    {
        RuleFor(a => a.Name)
            .NotEmpty().MaximumLength(255)
            .WithName($"{prefix}.Name");

        RuleFor(a => a.Phone)
            .NotEmpty().MaximumLength(20)
            .WithName($"{prefix}.Phone");

        RuleFor(a => a.Address)
            .NotEmpty()
            .WithName($"{prefix}.Address");
    }
}
