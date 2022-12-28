using Hushify.Api.Options;
using Hushify.Api.Persistence.Entities;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Sodium;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Hushify.Api.Features.Identity.Services;

public interface ITokenGenerator
{
    (string accessTokenNonce, string encAccessToken, string serverPublicKey) GenerateAccessToken(
        IEnumerable<Claim> claims, string publicKey);

    RefreshToken GenerateRefreshToken(string userAgent);
}

public sealed class TokenGenerator : ITokenGenerator
{
    private readonly ConfigOptions _options;
    private readonly RsaSecurityKey _privateSecurityKey;

    public TokenGenerator(IOptions<ConfigOptions> options, CryptoKeys cryptoKeys)
    {
        _options = options.Value;
        _privateSecurityKey = cryptoKeys.PrivateSecurityKey;
    }

    public (string accessTokenNonce, string encAccessToken, string serverPublicKey) GenerateAccessToken(
        IEnumerable<Claim> claims, string publicKey)
    {
        // generate token that is valid for 30 minutes
        var tokenDescriptor = new JwtSecurityToken
        (
            _options.Jwt.ValidIssuer, _options.Jwt.ValidAudience, claims,
            expires: DateTime.UtcNow.AddMinutes(_options.Jwt.TokenValidityInMins),
            signingCredentials: new SigningCredentials(_privateSecurityKey, SecurityAlgorithms.RsaSha256)
        );

        var handler = new JwtSecurityTokenHandler();
        var token = handler.WriteToken(tokenDescriptor);

        var nonce = PublicKeyBox.GenerateNonce();
        var cipher = PublicKeyBox.Create(Encoding.UTF8.GetBytes(token), nonce,
            WebEncoders.Base64UrlDecode(_options.CryptoBoxKeyPair.PrivateKey),
            WebEncoders.Base64UrlDecode(publicKey));

        return (WebEncoders.Base64UrlEncode(nonce), WebEncoders.Base64UrlEncode(cipher),
            _options.CryptoBoxKeyPair.PublicKey);
    }

    public RefreshToken GenerateRefreshToken(string userAgent)
    {
        var randomBytes = new byte[64];
        RandomNumberGenerator.Fill(randomBytes);

        return new RefreshToken
        {
            Id = Guid.NewGuid().ToString(),
            Token = Convert.ToBase64String(randomBytes),
            Expires = DateTimeOffset.UtcNow.AddDays(_options.RefreshToken.TimeToLiveInDays),
            Created = DateTimeOffset.UtcNow,
            CreatedByUserAgent = userAgent
        };
    }
}