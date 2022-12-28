using Hushify.Api.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace Hushify.Api.Features.Identity.Services;

public sealed class CryptoKeys
{
    public readonly RsaSecurityKey PrivateSecurityKey;
    public readonly RsaSecurityKey PublicSecurityKey;

    public CryptoKeys(IOptions<ConfigOptions> options)
    {
        PublicSecurityKey = GetPublicSecurityKey(options.Value.RSAKeyPair.PublicKey);
        PrivateSecurityKey = GetPrivateSecurityKey(options.Value.RSAKeyPair.PrivateKey);
    }

    private static RsaSecurityKey GetPublicSecurityKey(string publicKey)
    {
        var rsa = RSA.Create();

        rsa.ImportRSAPublicKey(
            Convert.FromBase64String(publicKey),
            out _
        );

        return new RsaSecurityKey(rsa);
    }

    private static RsaSecurityKey GetPrivateSecurityKey(string privateKey)
    {
        var rsa = RSA.Create();
        rsa.ImportRSAPrivateKey(
            Convert.FromBase64String(privateKey),
            out _
        );

        return new RsaSecurityKey(rsa);
    }
}