using Hushify.Api.Constants;

namespace Hushify.Api.Exceptions;

public sealed class AppException : Exception
{
    public AppException(string message) : this(message, Enumerable.Empty<AppFailure>()) { }

    public AppException(string message, IEnumerable<AppFailure> failures) : base(message) => Failures = failures;

    public AppException(string message, Exception innerException) : base(message, innerException) =>
        Failures = Enumerable.Empty<AppFailure>();

    public AppException(string message, string[] failures) : base(message) =>
        Failures = failures.Select(f => new AppFailure(ErrorConstants.Errors, f));

    private AppException() { }

    public IEnumerable<AppFailure> Failures { get; } = default!;
}

public sealed record AppFailure(string Name, string Message);