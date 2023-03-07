using FluentValidation;
using Hushify.Api.Features.Identity.Entities;
using Hushify.Api.Filters;
using Hushify.Api.Options;
using Hushify.Api.Services;
using MassTransit;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Hushify.Api.Features.Identity.Endpoints;

public static class ForgotPassword
{
    public static IEndpointRouteBuilder MapResetPasswordEndpoints(this RouteGroupBuilder routes)
    {
        routes.WithParameterValidation(typeof(ResetPasswordRequest));

        routes.MapPost("/reset-password", ResetPasswordHandler);
        return routes;
    }

    private static async Task<Results<Ok, ValidationProblem>> ResetPasswordHandler(
        ResetPasswordRequest req, IOptions<ConfigOptions> options,
        IEmailService emailService, UserManager<AppUser> userManager, IBus bus, CancellationToken ct)
    {
        var user = await userManager.FindByEmailAsync(req.Email);
        if (user is null)
        {
            await Task.Delay(50, ct);
            return TypedResults.Ok();
        }

        var resetPasswordToken = await userManager.GenerateTwoFactorTokenAsync(user,
            TokenOptions.DefaultEmailProvider);

        var body = $@"<div>
            <div>Here's your reset password code:</div>
            <h1>{resetPasswordToken}</h1>
        </div>";

        await emailService.SendEmailAsync(options.Value.Email.From, user.Email!, "Reset password code", body, true, ct);
        return TypedResults.Ok();
    }
}

public sealed record ResetPasswordRequest(string Email);

public sealed class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().WithMessage((request, _) => $"{request.Email} can not be empty.")
            .EmailAddress().WithMessage((request, _) => $"{request.Email} is not a valid email address.");
    }
}