using Hushify.Api.Filters;

namespace Hushify.Api.Features.Identity.Entities;

public sealed record KeyPairBundle(string Nonce, string PublicKey, string EncryptedPrivateKey) : ISkipWorkspaceFilter;