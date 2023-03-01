using Hushify.Api.Constants;
using Hushify.Api.Exceptions;
using Hushify.Api.Features.Identity.Endpoints;
using Hushify.Api.Features.Identity.Entities;
using Hushify.Api.Features.Identity.Services;
using Hushify.Api.Features.Identity.Validators;
using Hushify.Api.Options;
using Hushify.Api.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace Hushify.Api.Features.Identity;

public static class IdentityExtensions
{
    private const string RefreshToken = "refresh-token";

    public static void AddEfBasedIdentity(this WebApplicationBuilder builder, JwtOptions jwtOptions)
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

                // options.MapInboundClaims = false;
                options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
                // options.Authority = opts.Jwt.Authority;
                options.SaveToken = true;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ClockSkew = TimeSpan.Zero,
                    ValidAudience = jwtOptions.ValidAudience,
                    ValidIssuer = jwtOptions.ValidIssuer,
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
    }

    public static void MapIdentityEndpoints(this IEndpointRouteBuilder routes)
    {
        var identityRoutes = routes.MapGroup("/identity");
        identityRoutes.WithTags("Identity");
        identityRoutes.RequireRateLimiting(AppConstants.IpRateLimit);

        identityRoutes.MapRegisterEndpoints();
        identityRoutes.MapLoginEndpoints();
        identityRoutes.MapRefreshEndpoints();
        identityRoutes.MapResetPasswordEndpoints();
        identityRoutes.MapLogoutEndpoints();
        identityRoutes.MapSessionEndpoints();
    }

    private static CookieOptions GetCookieOptions(DateTimeOffset expires, string domain) => new()
    {
        IsEssential = true,
        Secure = true,
        HttpOnly = true,
        SameSite = SameSiteMode.Lax,
        Path = "/identity/refresh",
        Expires = expires,
        Domain = domain
    };

    public static bool GetRefreshTokenCookie(this HttpContext ctx, out string? token)
    {
        try
        {
            if (!ctx.Request.Cookies.TryGetValue(RefreshToken, out token))
            {
                return false;
            }

            var dataProtectionProvider = ctx.RequestServices.GetRequiredService<IDataProtectionProvider>();
            var protector = dataProtectionProvider.CreateProtector(RefreshToken);

            token = protector.Unprotect(token);
            return true;
        }
        catch (CryptographicException)
        {
            token = null;
            return false;
        }
    }

    public static void SetRefreshTokenCookie(this HttpContext ctx, string refreshToken, int ttlInDays, string domain)
    {
        var dataProtectionProvider = ctx.RequestServices.GetRequiredService<IDataProtectionProvider>();
        var protector = dataProtectionProvider.CreateProtector(RefreshToken);

        var cookieOptions = GetCookieOptions(DateTimeOffset.UtcNow.AddDays(ttlInDays), domain);
        ctx.Response.Cookies.Append(RefreshToken, protector.Protect(refreshToken), cookieOptions);
    }

    public static bool DeleteRefreshTokenCookie(this HttpContext ctx, string domain, out string? token)
    {
        try
        {
            if (!ctx.Request.Cookies.TryGetValue(RefreshToken, out token))
            {
                return false;
            }

            var dataProtectionProvider = ctx.RequestServices.GetRequiredService<IDataProtectionProvider>();
            var protector = dataProtectionProvider.CreateProtector(RefreshToken);

            token = protector.Unprotect(token);
            return true;
        }
        catch (CryptographicException)
        {
            token = null;
            return false;
        }
        finally
        {
            ctx.Response.Cookies.Delete(RefreshToken, GetCookieOptions(DateTimeOffset.MinValue, domain));
        }
    }

    public static IEnumerable<Claim> GetAccessTokenClaims(this AppUser user)
    {
        return new Claim[]
        {
            new(AppClaimTypes.Jti, Guid.NewGuid().ToString()),
            new(AppClaimTypes.Sub, user.Id.ToString()),
            new(AppClaimTypes.Name, user.Email ?? throw new AppException("User email was null.")),
            new(AppClaimTypes.Workspace, user.WorkspaceId.ToString())
        };
    }
}