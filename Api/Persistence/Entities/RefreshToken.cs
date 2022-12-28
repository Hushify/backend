using Hushify.Api.Persistence.Filters;

namespace Hushify.Api.Persistence.Entities;

public sealed class RefreshToken : ISkipWorkspaceFilter
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string Token { get; set; } = default!;

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
}