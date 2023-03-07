using Hushify.Api.Features.Identity.Entities;
using Hushify.Api.Features.Identity.Messaging.Events;
using Hushify.Api.Options;
using Hushify.Api.Services;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Hushify.Api.Features.Identity.Messaging.Handlers;

public sealed class SendLoginCode : IConsumer<InitiateLogin>
{
    private readonly IEmailService _emailService;
    private readonly EmailOptions _options;
    private readonly UserManager<AppUser> _userManager;

    public SendLoginCode(IEmailService emailService, IOptionsMonitor<ConfigOptions> options,
        UserManager<AppUser> userManager)
    {
        _emailService = emailService;
        _userManager = userManager;
        _options = options.CurrentValue.Email;
    }

    public async Task Consume(ConsumeContext<InitiateLogin> context)
    {
        var user = await _userManager.FindByIdAsync(context.Message.Id.ToString());
        if (user is null)
        {
            return;
        }

        var loginConfirmationCode = await _userManager.GenerateTwoFactorTokenAsync(user,
            TokenOptions.DefaultEmailProvider);

        var body = $@"<div>
            <div>Here's your login confirmation code:</div>
            <h1>{loginConfirmationCode}</h1>
        </div>";

        await _emailService.SendEmailAsync(_options.From, user.Email!, "Login Code", body, true,
            context.CancellationToken);
    }
}