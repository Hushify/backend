using FluentValidation;
using Hushify.Api;
using Hushify.Api.Constants;
using Hushify.Api.Exceptions;
using Hushify.Api.Extensions;
using Hushify.Api.Features.Drive;
using Hushify.Api.Features.Drive.Messaging.Handlers;
using Hushify.Api.Features.Identity;
using Hushify.Api.Options;
using Hushify.Api.Persistence;
using Hushify.Api.Resolvers;
using Hushify.Api.Services;
using MassTransit;
using Microsoft.FeatureManagement;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.AddSerilog();

var configSection = builder.Configuration.GetSection(ConfigOptions.Config);
builder.Services.Configure<ConfigOptions>(configSection);

var configOptions = configSection.Get<ConfigOptions>() ?? throw new AppException("Missing config.");

builder.Services.AddHttpContextAccessor();

builder.AddEfAndDataProtection();
builder.AddEfBasedIdentity(configOptions.Jwt);
builder.AddDrive(configOptions.AWS);
builder.AddStripeServices(configOptions.Stripe);

// FluentValidation and Validators
ValidatorOptions.Global.PropertyNameResolver = CamelCasePropertyNameResolver.ResolvePropertyName;
builder.Services.AddValidatorsFromAssembly(Assembly.GetAssembly(typeof(IApiMarker)));

var isSmtpUsernameEmpty = string.IsNullOrWhiteSpace(configOptions.Email.Username);

// FluentEmail
builder.Services
    .AddFluentEmail(configOptions.Email.From)
    .AddRazorRenderer()
    .AddSmtpSender(() => new SmtpClient(configOptions.Email.Host, configOptions.Email.Port)
    {
        Credentials = isSmtpUsernameEmpty
            ? null
            : new NetworkCredential(configOptions.Email.Username, configOptions.Email.Password),
        EnableSsl = !isSmtpUsernameEmpty,
        DeliveryMethod = SmtpDeliveryMethod.Network
    });

builder.Services.AddScoped<IEmailService, FluentEmailService>();
builder.Services.AddScoped<ICaptchaService, CaptchaService>();

// Open Api
builder.Services.AddEndpointsApiExplorer();

// Add and Configure Swagger
builder.Services.AddSwaggerGen();
builder.Services.Configure<SwaggerGeneratorOptions>(opts => opts.InferSecuritySchemes = true);

// Add MassTransit w/ Rabbit
builder.Services.AddMassTransit(config =>
{
    config.AddConsumersFromNamespaceContaining<IApiMarker>();

    config.UsingRabbitMq((ctx, cfg) =>
    {
        var rabbitOptions = configOptions.Rabbit;
        cfg.Host(rabbitOptions.Host, rabbitOptions.VirtualHost, h =>
        {
            h.Username(rabbitOptions.Username);
            h.Password(rabbitOptions.Password);
        });

        cfg.UseMessageRetry(r => r.Immediate(100));
        cfg.ConfigureEndpoints(ctx);
    });
});

// Add MassTransit w/ SQS
if (configOptions.AWS.QueueName is not null && configOptions.AWS.QueueRegion is not null)
{
    builder.Services.AddMassTransit<ISQSBus>(config =>
    {
        config.AddConsumer<MultipartS3Event>();

        config.UsingAmazonSqs((ctx, cfg) =>
        {
            var awsOptions = configOptions.AWS;
            cfg.Host(awsOptions.QueueRegion, h =>
            {
                h.AccessKey(awsOptions.QueueAccessKey);
                h.SecretKey(awsOptions.QueueSecretKey);
            });

            cfg.ReceiveEndpoint(awsOptions.QueueName,
                e =>
                {
                    e.ConfigureConsumeTopology = false;
                    e.PublishFaults = false;
                    e.Durable = true;
                    e.AutoStart = true;

                    e.ClearSerialization();
                    e.UseRawJsonSerializer();
                    e.ConfigureConsumer<MultipartS3Event>(ctx);
                });
        });
    });
}

builder.Services.AddFeatureManagement();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy(AppConstants.IpRateLimit, context =>
    {
        var ipAddress = context.GetUserIpAddress();

        return RateLimitPartition.GetSlidingWindowLimiter(ipAddress, key =>
            new SlidingWindowRateLimiterOptions
            {
                Window = TimeSpan.FromSeconds(10),
                PermitLimit = 25,
                SegmentsPerWindow = 5,
                AutoReplenishment = true,
                QueueLimit = 2,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            });
    });

    options.AddPolicy(AppConstants.EmailCodeLimit, context =>
    {
        var ipAddress = context.GetUserIpAddress();

        return RateLimitPartition.GetSlidingWindowLimiter(ipAddress, key =>
            new SlidingWindowRateLimiterOptions
            {
                Window = TimeSpan.FromMinutes(1),
                PermitLimit = 12,
                SegmentsPerWindow = 2,
                AutoReplenishment = true,
                QueueLimit = 2,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            });
    });
});

builder.Services.AddCors(options =>
{
    options.DefaultPolicyName = "Cors";
    options.AddDefaultPolicy(policyBuilder =>
        policyBuilder.WithOrigins(
            configOptions.WebUrls.Select(origin => origin.ToString()).ToArray()
        ).AllowAnyHeader().AllowAnyMethod().AllowCredentials());
});

builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseSerilogRequestLogging();

app.UseCors("Cors");

app.UseSwagger();
app.UseSwaggerUI();

app.UseMiddleware<ErrorHandlerMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.UseRateLimiter();

app.MapHealthChecks("/health");
app.MapIdentityEndpoints();
app.MapDriveEndpoints();

app.Run();

// This is to enable testing
namespace Hushify.Api
{
    public class Program { }
}

public interface ISQSBus : IBus { }