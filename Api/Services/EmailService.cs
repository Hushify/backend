using FluentEmail.Core;

namespace Hushify.Api.Services;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body, bool isHtml,
        CancellationToken cancellationToken = default);

    Task SendTemplatedEmailAsync<T>(string to, string subject, string template, T model, bool isHtml,
        CancellationToken cancellationToken = default);
}

public sealed class FluentEmailService : IEmailService
{
    private readonly IFluentEmail _fluentEmail;

    public FluentEmailService(IFluentEmail fluentEmail) => _fluentEmail = fluentEmail;

    public Task SendEmailAsync(string to, string subject, string body, bool isHtml,
        CancellationToken cancellationToken = default) =>
        _fluentEmail.To(to).Subject(subject).Body(body, isHtml).SendAsync(cancellationToken);

    public Task SendTemplatedEmailAsync<T>(string to, string subject, string template, T model, bool isHtml,
        CancellationToken cancellationToken = default) =>
        _fluentEmail.To(to).Subject(subject).UsingTemplate(template, model, isHtml).SendAsync(cancellationToken);
}