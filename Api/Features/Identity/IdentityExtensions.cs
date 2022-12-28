using Hushify.Api.Constants;
using Hushify.Api.Features.Identity.Services;
using Hushify.Api.Options;
using Hushify.Api.Persistence;
using Hushify.Api.Persistence.Entities;
using Hushify.Api.Persistence.Validators;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace Hushify.Api.Features.Identity;

public static class IdentityExtensions
{
    public static WebApplicationBuilder AddEfBasedIdentity(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<ITokenGenerator, TokenGenerator>();
        builder.Services.AddSingleton<CryptoKeys>();

        // Identity Services
        var identityBuilder = builder.Services
            .AddIdentity<AppUser, AppRole>(options =>
            {
                options.Lockout.AllowedForNewUsers = true;
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = true;
            })
            .AddRoleValidator<WorkspaceRoleValidator>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        var userType = identityBuilder.UserType;
        var emailTokenProviderType = typeof(EmailTokenProvider<>).MakeGenericType(userType);
        identityBuilder.AddTokenProvider(TokenOptions.DefaultEmailProvider, emailTokenProviderType);

        // IdentityModelEventSource.ShowPII = builder.Environment.IsDevelopment();
        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

        builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                var sp = builder.Services.BuildServiceProvider();
                var opts = sp.GetRequiredService<IOptions<ConfigOptions>>().Value;

                // options.MapInboundClaims = false;
                options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
                // options.Authority = opts.Jwt.Authority;
                options.SaveToken = true;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ClockSkew = TimeSpan.Zero,
                    ValidAudience = opts.Jwt.ValidAudience,
                    ValidIssuer = opts.Jwt.ValidIssuer,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = sp.GetRequiredService<CryptoKeys>().PublicSecurityKey,
                    RequireExpirationTime = true,
                    ValidateLifetime = true,
                    NameClaimType = AppClaimTypes.Name,
                    RoleClaimType = AppClaimTypes.Role
                };

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = ctx => Task.CompletedTask,
                    OnForbidden = ctx => Task.CompletedTask,
                    OnChallenge = ctx => Task.CompletedTask
                };

                // options.Configuration = new OpenIdConnectConfiguration
                // {
                //     SigningKeys = { sp.GetRequiredService<CryptoKeys>().PublicSecurityKey }
                // };
            });

        builder.Services.AddAuthorization();

        return builder;
    }
}