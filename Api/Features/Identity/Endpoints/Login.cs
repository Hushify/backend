using FluentValidation;
using Hushify.Api.Constants;
using Hushify.Api.Exceptions;
using Hushify.Api.Extensions;
using Hushify.Api.Features.Identity.Entities;
using Hushify.Api.Features.Identity.Messaging.Events;
using Hushify.Api.Features.Identity.Services;
using Hushify.Api.Filters;
using Hushify.Api.Options;
using MassTransit;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Hushify.Api.Features.Identity.Endpoints;

public static class Login
{
    public static IEndpointRouteBuilder MapLoginEndpoints(this RouteGroupBuilder routes)
    {
        routes.WithParameterValidation(typeof(InitiateLoginRequest), typeof(ConfirmLoginRequest));

        routes.MapPost("/login", InitiateLoginHandler).RequireRateLimiting(AppConstants.EmailCodeLimit);
        routes.MapPost("/login-confirm", ConfirmLoginHandler);
        return routes;
    }

    private static async Task<Results<Ok, ValidationProblem>> InitiateLoginHandler(
        InitiateLoginRequest req,
        UserManager<AppUser> userManager, IOptions<ConfigOptions> options,
        IBus bus, CancellationToken ct)
    {
        var user = await userManager.FindByEmailAsync(req.Email);
        if (user is null)
        {
            return TypedResults.Ok();
        }

        if (!user.EmailConfirmed)
        {
            return TypedResults.ValidationProblem(new Dictionary<string, string[]>
            {
                { "errors", new[] { "You have not confirmed your email yet." } }
            });
        }

        await bus.Publish(new InitiateLogin(user.Id), ct);
        return TypedResults.Ok();
    }

    private static async Task<Results<Ok<ConfirmLoginResponse>, ValidationProblem>> ConfirmLoginHandler(
        ConfirmLoginRequest req, ITokenGenerator tokenGenerator,
        IHttpContextAccessor ctxAccessor, UserManager<AppUser> userManager, IOptions<ConfigOptions> options,
        IBus bus, CancellationToken ct)
    {
        var user = await userManager.FindByEmailAsync(req.Email);
        if (user is null)
        {
            return TypedResults.ValidationProblem(new Dictionary<string, string[]>
                { { "errors", new[] { "Invalid email or code." } } });
        }

        if (!user.EmailConfirmed)
        {
            return TypedResults.ValidationProblem(new Dictionary<string, string[]>
                { { "errors", new[] { "Wrong email or code." } } });
        }

        var isCodeValid =
            await userManager.VerifyTwoFactorTokenAsync(user, TokenOptions.DefaultEmailProvider, req.Code);
        if (!isCodeValid)
        {
            return TypedResults.ValidationProblem(new Dictionary<string, string[]>
                { { "errors", new[] { "Wrong email or code." } } });
        }

        var ctx = ctxAccessor.HttpContext ?? throw new AppException("HttpContext was null.");

        var userAgent = ctx.GetUserAgent();

        var (accessTokenNonce, encAccessToken, serverPublicKey) =
            tokenGenerator.GenerateAccessToken(user.GetAccessTokenClaims(), user.AsymmetricEncKeyBundle!.PublicKey);
        var refreshToken = tokenGenerator.GenerateRefreshToken(userAgent);

        user.RefreshTokens.Add(refreshToken);
        await userManager.UpdateAsync(user);

        ctx.SetRefreshTokenCookie(refreshToken.Token, options.Value.RefreshToken.TimeToLiveInDays,
            options.Value.ApiUrl.Domain);

        return TypedResults.Ok(new ConfirmLoginResponse(user.Salt!, encAccessToken, accessTokenNonce, serverPublicKey,
            user.MasterKeyBundle!.Nonce, user.MasterKeyBundle.EncKey, user.AsymmetricEncKeyBundle.Nonce,
            user.AsymmetricEncKeyBundle.PublicKey, user.AsymmetricEncKeyBundle.EncPrivateKey,
            user.SigningKeyBundle!.Nonce,
            user.SigningKeyBundle.PublicKey, user.SigningKeyBundle.EncPrivateKey));
    }
}

public sealed record InitiateLoginRequest(string Email);

public sealed class InitiateLoginRequestValidator : AbstractValidator<InitiateLoginRequest>
{
    public InitiateLoginRequestValidator()
    {
        RuleFor(c => c.Email)
            .NotEmpty()
            .WithMessage("Email address can not be empty.")
            .EmailAddress()
            .WithMessage((request, _) => $"{request.Email} is not a valid email address.");
    }
}

public sealed record ConfirmLoginRequest(string Email, string Code);

public sealed class ConfirmLoginRequestValidator : AbstractValidator<ConfirmLoginRequest>
{
    public ConfirmLoginRequestValidator()
    {
        RuleFor(c => c).NotNull();
        RuleFor(c => c.Email)
            .NotEmpty().WithMessage((request, _) => $"{request.Email} can not be empty.")
            .EmailAddress().WithMessage((request, _) => $"{request.Email} is not a valid email.");
        RuleFor(c => c.Code).NotEmpty();
    }
}

public sealed record ConfirmLoginResponse(string Salt, string EncAccessToken, string EncAccessTokenNonce,
    string ServerPublicKey, string MasterKeyNonce, string EncMasterKey, string AsymmetricEncKeyNonce,
    string AsymmetricEncPublicKey, string EncAsymmetricPrivateKey, string SigningKeyNonce, string SigningPublicKey,
    string EncSigningPrivateKey);