using Hushify.Api.Persistence.Filters;
using Microsoft.AspNetCore.Identity;

namespace Hushify.Api.Persistence.Entities;

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
    public string? Salt { get; set; }

    public SecretKeyBundle? MasterKeyBundle { get; set; }
    public SecretKeyBundle? RecoveryMasterKeyBundle { get; set; }
    public SecretKeyBundle? RecoveryKeyBundle { get; set; }

    public KeyPairBundle? AsymmetricEncKeyBundle { get; set; }
    public KeyPairBundle? SigningKeyBundle { get; set; }

    public List<RefreshToken> RefreshTokens { get; set; } = new();

    public Workspace Workspace { get; set; } = default!;
    public Guid WorkspaceId { get; set; }
}

public sealed record SecretKeyBundle(string Nonce, string EncKey) : ISkipWorkspaceFilter;

public sealed record KeyPairBundle(string Nonce, string PublicKey, string EncPrivateKey) : ISkipWorkspaceFilter;