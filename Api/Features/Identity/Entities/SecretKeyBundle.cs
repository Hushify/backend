using Hushify.Api.Filters;

namespace Hushify.Api.Features.Identity.Entities;

public sealed record SecretKeyBundle(string Nonce, string EncryptedKey) : ISkipWorkspaceFilter;