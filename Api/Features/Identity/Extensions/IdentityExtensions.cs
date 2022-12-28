using Hushify.Api.Constants;
using Hushify.Api.Exceptions;
using Hushify.Api.Persistence.Entities;
using Microsoft.AspNetCore.DataProtection;
using System.Security.Claims;
using System.Security.Cryptography;

namespace Hushify.Api.Features.Identity.Extensions;

public static class IdentityExtensions
{
    private const string RefreshToken = "refresh-token";

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

    public static void DeleteRefreshTokenCookie(this HttpContext ctx, string domain) =>
        ctx.Response.Cookies.Delete(RefreshToken, GetCookieOptions(DateTimeOffset.MinValue, domain));

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