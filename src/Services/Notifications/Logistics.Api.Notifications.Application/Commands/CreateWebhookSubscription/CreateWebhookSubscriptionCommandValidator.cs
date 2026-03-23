using FluentValidation;
using Logistics.Api.Notifications.Application.Common;

namespace Logistics.Api.Notifications.Application.Commands.CreateWebhookSubscription;

public sealed class CreateWebhookSubscriptionCommandValidator : AbstractValidator<CreateWebhookSubscriptionCommand>
{
    public CreateWebhookSubscriptionCommandValidator()
    {
        RuleFor(x => x.RequestingUserId).NotEmpty();
        RuleFor(x => x.CallbackUrl)
            .NotEmpty()
            .MaximumLength(2048)
            .Must(CallbackUrlValidator.IsValid)
            .WithMessage("Callback URL phải là HTTPS hợp lệ hoặc HTTP localhost.");

        RuleFor(x => x.Events)
            .NotEmpty();

        RuleForEach(x => x.Events)
            .Must(WebhookEventNames.Supported.Contains)
            .WithMessage("Webhook event không được hỗ trợ.");
    }
}