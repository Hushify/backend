using Hushify.Api.Constants;
using Hushify.Api.Features.Identity.Messaging.Events;
using Hushify.Api.Persistence.Entities;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.FeatureManagement;

namespace Hushify.Api.Features.Identity.Messaging.Handlers;

public sealed class CreateStripeCustomer : IConsumer<EmailConfirmed>
{
    private readonly IFeatureManager _featureManager;
    private readonly UserManager<AppUser> _userManager;

    public CreateStripeCustomer(UserManager<AppUser> userManager, IFeatureManager featureManager)
    {
        _userManager = userManager;
        _featureManager = featureManager;
    }

    public async Task Consume(ConsumeContext<EmailConfirmed> context)
    {
        var stripeEnabled = await _featureManager.IsEnabledAsync(FeatureConstants.Stripe);
    }
}