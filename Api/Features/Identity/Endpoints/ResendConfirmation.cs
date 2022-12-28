using FluentValidation;
using Hushify.Api.Constants;
using Hushify.Api.Persistence.Entities;
using MassTransit;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;

namespace Hushify.Api.Features.Identity.Endpoints;

public static class ResendConfirmation
{
    public static IEndpointRouteBuilder MapResendConfirmationEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapPost("/resend-confirmation", ResendConfirmationHandler)
            .RequireRateLimiting(AppConstants.EmailCodeLimit);
        ;
        return routes;
    }

    private static async Task<Results<Ok<ResendConfirmationResponse>, ValidationProblem>> ResendConfirmationHandler(
        ResendConfirmationRequest req, IValidator<ResendConfirmationRequest> validator, IBus bus,
        UserManager<AppUser> userManager, CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(req, ct);
        if (!validationResult.IsValid)
        {
            return TypedResults.ValidationProblem(validationResult.ToDictionary());
        }

        var userId = Guid.NewGuid();
        var user = await userManager.FindByEmailAsync(req.Email);
        if (user is null)
        {
            await Task.Delay(50, ct);
            return TypedResults.Ok(new ResendConfirmationResponse(userId));
        }

        await bus.Publish(new Messaging.Events.ResendConfirmation(user.Id), ct);
        return TypedResults.Ok(new ResendConfirmationResponse(user.Id));
    }
}

public sealed record ResendConfirmationRequest(string Email);

public sealed class ResendConfirmationRequestValidator : AbstractValidator<ResendConfirmationRequest>
{
    public ResendConfirmationRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().WithMessage("Email address can not be empty.").EmailAddress()
            .WithMessage((request, _) => $"{request.Email} is not a valid email address.");
    }
}

public sealed record ResendConfirmationResponse(Guid UserId);