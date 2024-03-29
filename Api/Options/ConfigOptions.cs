namespace Hushify.Api.Options;

public sealed class ConfigOptions
{
    public const string Config = nameof(Config);

    public EmailOptions Email { get; set; } = default!;
    public RabbitOptions Rabbit { get; set; } = default!;
    public KeyPair RSAKeyPair { get; set; } = default!;
    public KeyPair CryptoBoxKeyPair { get; set; } = default!;
    public JwtOptions Jwt { get; set; } = default!;
    public RefreshTokenOptions RefreshToken { get; set; } = default!;
    public StripeOptions Stripe { get; set; } = default!;
    public AWSOptions AWS { get; set; } = default!;
    public AppUrl ApiUrl { get; set; } = default!;
    public AppUrl[] WebUrls { get; set; } = default!;
}

public sealed record AppUrl(string Scheme, string Domain)
{
    public override string ToString() => $"{Scheme}://{Domain}";
}

public sealed record RabbitOptions(string Host, string VirtualHost, string Username,
    string Password);

public sealed record JwtOptions(string ValidAudience, string ValidIssuer, int TokenValidityInMins);

public sealed record KeyPair(string PrivateKey, string PublicKey);

public sealed record RefreshTokenOptions(int TimeToLiveInDays);

public sealed class EmailOptions
{
    public string From { get; set; } = default!;
    public string Host { get; set; } = default!;
    public int Port { get; set; } = default!;
    public string Username { get; set; } = default!;
    public string Password { get; set; } = default!;
}

public sealed class StripeOptions
{
    public bool IsEnabled { get; set; } = false;
    public string PublishableKey { get; set; } = default!;
    public string SecretKey { get; set; } = default!;
}

public sealed record AWSOptions(string? KeyId, string? ServiceUrl, string BucketName,
    string AccessKey, string SecretKey, string? Region, string? CloudFrontServiceUrl,
    bool IsCloudFrontEnabled, bool PathStyle, string? QueueName, string? QueueRegion,
    string? QueueAccessKey, string? QueueSecretKey);