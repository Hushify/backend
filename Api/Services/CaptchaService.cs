using Hushify.Api.Options;
using Microsoft.Extensions.Options;

namespace Hushify.Api.Services;

public interface ICaptchaService
{
    Task<bool> ValidateCaptchaAsync(string captcha, CancellationToken cancellationToken);
}

public class CaptchaService : ICaptchaService
{
    private readonly HttpClient _httpClient;
    private readonly ConfigOptions _options;

    public CaptchaService(IHttpClientFactory httpClientFactory, IOptions<ConfigOptions> options)
    {
        _options = options.Value;
        _httpClient = httpClientFactory.CreateClient();
    }

    public async Task<bool> ValidateCaptchaAsync(string captcha, CancellationToken cancellationToken)
    {
        var response = await _httpClient.PostAsJsonAsync("https://challenges.cloudflare.com/turnstile/v0/siteverify",
            new CaptchaPostRequest(_options.TurnstileSecretKey, captcha), cancellationToken);

        var responseContent =
            await response.Content.ReadFromJsonAsync<CaptchaResponse>(cancellationToken: cancellationToken);

        return responseContent is not null && responseContent.Success;
    }
}

public sealed record CaptchaPostRequest(string Secret, string Response);

public sealed record CaptchaResponse(bool Success);