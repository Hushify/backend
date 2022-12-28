using Hushify.Api.Features.Identity.Messaging.Events;
using Hushify.Api.Persistence.Entities;
using Hushify.Api.Services;
using MassTransit;
using Microsoft.AspNetCore.Identity;

namespace Hushify.Api.Features.Identity.Messaging.Handlers;

public sealed class ResendConfirmationCode : IConsumer<ResendConfirmation>
{
    private readonly IEmailService _emailService;
    private readonly UserManager<AppUser> _userManager;

    public ResendConfirmationCode(IEmailService emailService, UserManager<AppUser> userManager)
    {
        _emailService = emailService;
        _userManager = userManager;
    }

    public async Task Consume(ConsumeContext<ResendConfirmation> context)
    {
        var user = await _userManager.FindByIdAsync(context.Message.Id.ToString());
        if (user is null || user.EmailConfirmed)
        {
            return;
        }

        var emailConfirmationToken = await _userManager.GenerateTwoFactorTokenAsync(user,
            TokenOptions.DefaultEmailProvider);

        var body = $@"<div>
            <div>Here's your registration confirmation code:</div>
            <h1>{emailConfirmationToken}</h1>
        </div>";

        await _emailService.SendEmailAsync(user.Email!, "Confirm your email", body, true, context.CancellationToken);
    }
}