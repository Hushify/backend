using Hushify.Api.Filters;

namespace Hushify.Api.Features.Identity.Entities;

public sealed record UserCryptoProperties(string Salt, SecretKeyBundle MasterKeyBundle,
    SecretKeyBundle RecoveryMasterKeyBundle,
    SecretKeyBundle RecoveryKeyBundle, KeyPairBundle AsymmetricKeyBundle,
    KeyPairBundle SigningKeyBundle) : ISkipWorkspaceFilter;