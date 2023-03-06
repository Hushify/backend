using Hushify.Api.Features.Identity.Messaging.Events;
using Hushify.Api.Features.Identity.Services;
using MassTransit;

namespace Hushify.Api.Features.Identity.Messaging.Handlers;

public sealed class CreateStripeCustomer : IConsumer<EmailConfirmed>
{
    private readonly IStripeService _stripeService;

    public CreateStripeCustomer(IStripeService stripeService) => _stripeService = stripeService;

    public async Task Consume(ConsumeContext<EmailConfirmed> context)
    {
        await _stripeService.CreateCustomerAsync(context.Message.Id, context.CancellationToken);
    }
}