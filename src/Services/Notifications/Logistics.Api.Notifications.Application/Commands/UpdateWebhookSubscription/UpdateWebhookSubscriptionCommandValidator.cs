using FluentValidation;
using Logistics.Api.Notifications.Application.Common;

namespace Logistics.Api.Notifications.Application.Commands.UpdateWebhookSubscription;

public sealed class UpdateWebhookSubscriptionCommandValidator : AbstractValidator<UpdateWebhookSubscriptionCommand>
{
    public UpdateWebhookSubscriptionCommandValidator()
    {
        RuleFor(x => x.RequestingUserId).NotEmpty();
        RuleFor(x => x.SubscriptionId).NotEmpty();
        RuleFor(x => x.CallbackUrl)
            .NotEmpty()
            .MaximumLength(2048)
            .Must(CallbackUrlValidator.IsValid)
            .WithMessage("Callback URL phải là HTTPS hợp lệ hoặc HTTP localhost.");
        RuleFor(x => x.Events).NotEmpty();
        RuleForEach(x => x.Events)
            .Must(WebhookEventNames.Supported.Contains)
            .WithMessage("Webhook event không được hỗ trợ.");
    }
}