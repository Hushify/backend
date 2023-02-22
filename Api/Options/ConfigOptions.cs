namespace Hushify.Api.Options;

public sealed class ConfigOptions
{
    public const string Config = nameof(Config);

    public EmailOptions Email { get; set; } = default!;
    public RedisOptions Redis { get; set; } = default!;
    public KeyPair RSAKeyPair { get; set; } = default!;
    public KeyPair CryptoBoxKeyPair { get; set; } = default!;
    public JwtOptions Jwt { get; set; } = default!;
    public RefreshTokenOptions RefreshToken { get; set; } = default!;
    public StripeOptions Stripe { get; set; } = default!;
    public AWSOptions AWS { get; set; } = default!;

    public string RootDomain { get; set; } = default!;
    public AppUrl ApiUrl { get; set; } = default!;
    public AppUrl WebUrl { get; set; } = default!;
}

public sealed record AppUrl(string Scheme, string Domain)
{
    public override string ToString() => $"{Scheme}://{Domain}";
}

public sealed record RedisOptions(string Host, string VirtualHost, string Username, string Password);

public sealed record JwtOptions(string Authority, string ValidAudience,
    string ValidIssuer, int TokenValidityInMins);

public sealed record KeyPair(string PrivateKey, string PublicKey);

public sealed record RefreshTokenOptions(int TimeToLiveInDays);

public sealed class EmailOptions
{
    public string FromName { get; set; } = default!;
    public string FromAddress { get; set; } = default!;

    public string LocalDomain { get; set; } = default!;

    public string Host { get; set; } = default!;
    public int Port { get; set; } = default!;

    public string Username { get; set; } = default!;
    public string Password { get; set; } = default!;
}

public sealed class StripeOptions
{
    public string PublishableKey { get; set; } = default!;
    public string SecretKey { get; set; } = default!;
    public string PriceId { get; set; } = default!;
}

public sealed record AWSOptions(string KeyId, string ServiceUrl, string BucketName, string AccessKey, string SecretKey);