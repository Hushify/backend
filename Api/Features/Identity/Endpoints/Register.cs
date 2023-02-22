using FluentValidation;
using Hushify.Api.Constants;
using Hushify.Api.Exceptions;
using Hushify.Api.Extensions;
using Hushify.Api.Features.Drive.Entities;
using Hushify.Api.Features.Identity.Entities;
using Hushify.Api.Features.Identity.Messaging.Events;
using Hushify.Api.Features.Identity.Services;
using Hushify.Api.Filters;
using Hushify.Api.Options;
using Hushify.Api.Persistence;
using MassTransit;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Hushify.Api.Features.Identity.Endpoints;

public static class Register
{
    public static IEndpointRouteBuilder MapRegisterEndpoints(this RouteGroupBuilder routes)
    {
        routes.WithParameterValidation(typeof(RegisterRequest), typeof(ConfirmRequest));

        routes.MapPost("/register", RegisterHandler).RequireRateLimiting(AppConstants.EmailCodeLimit);
        routes.MapPost("/register-confirm", RegisterConfirmHandler);

        return routes;
    }

    private static async Task<Results<Ok, ValidationProblem>> RegisterHandler(
        RegisterRequest req, IBus bus,
        UserManager<AppUser> userManager, CancellationToken ct)
    {
        var user = await userManager.FindByEmailAsync(req.Email);

        switch (user)
        {
            case { EmailConfirmed: true }:
            {
                await Task.Delay(50, ct);
                return TypedResults.Ok();
            }
            case { EmailConfirmed: false }:
            {
                await bus.Publish(new ResendConfirmation(user.Id), ct);
                return TypedResults.Ok();
            }
        }

        user = new AppUser(Guid.NewGuid(), req.Email);

        var result = await userManager.CreateAsync(user);
        if (result.Errors.Any())
        {
            return TypedResults.ValidationProblem(result.Errors.ToDictionary(e => e.Code,
                e => new[] { e.Description }));
        }

        await bus.Publish(new UserCreated(user.Id), ct);
        return TypedResults.Ok();
    }

    private static async Task<Results<Ok, ValidationProblem>> RegisterConfirmHandler(ConfirmRequest req,
        IHttpContextAccessor ctxAccessor, UserManager<AppUser> userManager,
        IBus bus, IOptions<ConfigOptions> options, ITokenGenerator tokenGenerator,
        WorkspaceDbContext workspaceDbContext, CancellationToken ct)
    {
        var user = await userManager.FindByEmailAsync(req.Email);
        if (user is null)
        {
            return TypedResults.ValidationProblem(new Dictionary<string, string[]>
                { { "errors", new[] { "Invalid email or code." } } });
        }

        var result = await userManager.VerifyTwoFactorTokenAsync(user, TokenOptions.DefaultEmailProvider, req.Code);
        if (!result)
        {
            return TypedResults.ValidationProblem(new Dictionary<string, string[]>
                { { "errors", new[] { "Invalid email or code." } } });
        }

        user.EmailConfirmed = true;
        user.Salt = req.Salt;
        user.MasterKeyBundle = req.MasterKeyBundle;
        user.RecoveryMasterKeyBundle = req.RecoveryMasterKeyBundle;
        user.RecoveryKeyBundle = req.RecoveryKeyBundle;
        user.AsymmetricEncKeyBundle = req.AsymmetricEncKeyBundle; // TODO: Add validation
        user.SigningKeyBundle = req.SigningKeyBundle; // TODO: Add validation

        var ctx = ctxAccessor.HttpContext ?? throw new AppException("HttpContext was null.");

        var userAgent = ctx.GetUserAgent();

        var refreshToken = tokenGenerator.GenerateRefreshToken(userAgent);
        user.RefreshTokens.Add(refreshToken);

        await userManager.UpdateAsync(user);

        await workspaceDbContext.Folders.AddAsync(
            new FolderNode(user.WorkspaceId, $"workspace-{user.WorkspaceId}/", user.WorkspaceId, user.MasterKeyBundle,
                null, null),
            ct);
        await workspaceDbContext.SaveChangesAsync(ct);

        ctx.SetRefreshTokenCookie(refreshToken.Token, options.Value.RefreshToken.TimeToLiveInDays,
            options.Value.ApiUrl.Domain);

        await bus.Publish(new EmailConfirmed(user.Id), ct);
        return TypedResults.Ok();
    }
}

public sealed record RegisterRequest(string Email);

public sealed class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().WithMessage("Email address can not be empty.").EmailAddress()
            .WithMessage((request, _) => $"{request.Email} is not a valid email address.");
    }
}

public sealed record ConfirmRequest(string Email, string Code, string Salt, SecretKeyBundle MasterKeyBundle,
    SecretKeyBundle RecoveryMasterKeyBundle, SecretKeyBundle RecoveryKeyBundle, KeyPairBundle AsymmetricEncKeyBundle,
    KeyPairBundle SigningKeyBundle);

public sealed class ConfirmRequestValidator : AbstractValidator<ConfirmRequest>
{
    public ConfirmRequestValidator()
    {
        RuleFor(c => c).NotNull();
        RuleFor(c => c.Email).NotEmpty().EmailAddress();
        RuleFor(c => c.Code).NotEmpty();
        RuleFor(x => x.Salt).NotEmpty().Length(22);
        RuleFor(x => x.MasterKeyBundle).NotNull().WithName("Master Key Bundle");
        RuleFor(x => x.RecoveryMasterKeyBundle).NotNull().WithName("Recovery Master Key Bundle");
        RuleFor(x => x.RecoveryKeyBundle).NotNull().WithName("Recovery Key Bundle");
        RuleFor(x => x.AsymmetricEncKeyBundle).NotNull().WithName("Asymmetric Encryption Key Bundle");
        RuleFor(x => x.SigningKeyBundle).NotNull().WithName("Signing Key Bundle");
    }
}