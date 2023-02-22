using Hushify.Api.Features.Identity.Entities;
using Hushify.Api.Features.Identity.Messaging.Events;
using Hushify.Api.Services;
using MassTransit;
using Microsoft.AspNetCore.Identity;

namespace Hushify.Api.Features.Identity.Messaging.Handlers;

public sealed class SendConfirmationCode : IConsumer<UserCreated>, IConsumer<ResendConfirmation>
{
    private readonly IEmailService _emailService;
    private readonly UserManager<AppUser> _userManager;

    public SendConfirmationCode(IEmailService emailService, UserManager<AppUser> userManager)
    {
        _emailService = emailService;
        _userManager = userManager;
    }

    public async Task Consume(ConsumeContext<ResendConfirmation> context) =>
        await SendCodeAsync(context.Message.Id, context.CancellationToken);

    public async Task Consume(ConsumeContext<UserCreated> context) =>
        await SendCodeAsync(context.Message.Id, context.CancellationToken);

    public async Task Consume(ConsumeContext<InitiateLogin> context) =>
        await SendCodeAsync(context.Message.Id, context.CancellationToken);

    private async Task SendCodeAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
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

        await _emailService.SendEmailAsync(user.Email!, "Confirm your email", body, true, cancellationToken);
    }
}