using Hushify.Api.Features.Identity.Services;
using Hushify.Api.Filters;

namespace Hushify.Api.Features.Identity.Entities;

public sealed class RefreshToken : ISkipWorkspaceFilter
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string TokenHash { get; set; } = default!;

    public DateTimeOffset Created { get; set; } = DateTimeOffset.UtcNow;
    public string? CreatedByUserAgent { get; set; }

    public DateTimeOffset Expires { get; set; }

    public DateTimeOffset? Revoked { get; set; }
    public string? RevokedByUserAgent { get; set; }

    public string? ReplacedByTokenId { get; set; }
    public RefreshToken? ReplacedByToken { get; set; }

    public string? ReasonRevoked { get; set; }

    public bool IsExpired => DateTimeOffset.UtcNow >= Expires;
    public bool IsRevoked => Revoked != null;
    public bool IsActive => !IsRevoked && !IsExpired;

    public static void RevokeDescendantRefreshTokens(RefreshToken refreshToken, AppUser user,
        string userAgent, string reason)
    {
        if (refreshToken.ReplacedByToken is null)
        {
            return;
        }

        var childToken =
            user.RefreshTokens.SingleOrDefault(x =>
                x.TokenHash == refreshToken.ReplacedByToken.TokenHash);
        if (childToken!.IsActive)
        {
            RevokeRefreshToken(childToken, userAgent, reason);
        }
        else
        {
            RevokeDescendantRefreshTokens(childToken, user, userAgent, reason);
        }
    }

    public static (RefreshToken newRefreshToken, string token) RotateRefreshToken(
        RefreshToken refreshToken,
        ITokenGenerator tokenGenerator,
        string userAgent)
    {
        var (newRefreshToken, token) = tokenGenerator.GenerateRefreshToken(userAgent);
        RevokeRefreshToken(refreshToken, userAgent, "Replaced by new token", newRefreshToken.Id);
        return (newRefreshToken, token);
    }

    public static void RemoveOldRefreshTokens(AppUser user, int ttlInDays) =>
        user.RefreshTokens.RemoveAll(x =>
            !x.IsActive && x.Created.AddDays(ttlInDays) <= DateTime.UtcNow);

    public static void RevokeRefreshToken(RefreshToken token, string userAgent,
        string? reason = null, string? replacedByTokenId = null)
    {
        token.Revoked = DateTime.UtcNow;
        token.RevokedByUserAgent = userAgent;
        token.ReasonRevoked = reason;
        token.ReplacedByTokenId = replacedByTokenId;
    }
}