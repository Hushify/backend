using Hushify.Api.Filters;
using Microsoft.AspNetCore.Identity;

namespace Hushify.Api.Features.Identity.Entities;

public sealed class AppUser : IdentityUser<Guid>, IWorkspaceFilter
{
    private AppUser() { }

    public AppUser(Guid id, string email)
    {
        Id = id;
        Email = email;
        UserName = email;
        WorkspaceId = Guid.NewGuid();
        Workspace = new Workspace();
    }

    public string? StripeCustomerId { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ModifiedAt { get; set; }

    public UserCryptoProperties? CryptoProperties { get; set; }

    public List<RefreshToken> RefreshTokens { get; set; } = new();

    public Workspace Workspace { get; set; } = default!;
    public Guid WorkspaceId { get; set; }
}