using FluentValidation;
using Hushify.Api.Options;
using Hushify.Api.Persistence.Entities;
using Hushify.Api.Services;
using MassTransit;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Hushify.Api.Features.Identity.Endpoints;

public static class ForgotPassword
{
    public static IEndpointRouteBuilder MapForgotPasswordEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapPost("/forgot-password", ForgotPasswordHandler);
        return routes;
    }

    private static async Task<Results<Ok, ValidationProblem>> ForgotPasswordHandler(
        ForgotPasswordRequest req, IValidator<ForgotPasswordRequest> validator, IOptions<ConfigOptions> options,
        IEmailService emailService, UserManager<AppUser> userManager, IBus bus, CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(req, ct);
        if (!validationResult.IsValid)
        {
            return TypedResults.ValidationProblem(validationResult.ToDictionary());
        }

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

        await emailService.SendEmailAsync(user.Email!, "Reset password code", body, true, ct);
        return TypedResults.Ok();
    }
}

public sealed record ForgotPasswordRequest(string Email);

public sealed class ForgotPasswordRequestValidator : AbstractValidator<ForgotPasswordRequest>
{
    public ForgotPasswordRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().WithMessage((request, _) => $"{request.Email} can not be empty.")
            .EmailAddress().WithMessage((request, _) => $"{request.Email} is not a valid email address.");
    }
}