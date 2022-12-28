using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using System.Diagnostics;

namespace Hushify.Api.Extensions;

public static class WebApplicationExtensions
{
    /// <summary>
    ///     From: https://github.com/DamianEdwards/MinimalApiPlayground/blob/main/src/MinimalApiPlayground/Program.cs#L52
    ///     GitHub issue to support this in framework: https://github.com/dotnet/aspnetcore/issues/43831
    /// </summary>
    public static WebApplication AddAndCustomizeExceptionHandler(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            // Error handling
            app.UseExceptionHandler(new ExceptionHandlerOptions
            {
                AllowStatusCode404Response = true,
                ExceptionHandler = async context =>
                {
                    // The default exception handler always responds with status code 500 so we're overriding here to
                    // allow pass-through status codes from BadHttpRequestException.
                    // GitHub issue to support this in framework: https://github.com/dotnet/aspnetcore/issues/43831
                    var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();

                    if (exceptionHandlerFeature?.Error is BadHttpRequestException badRequestEx)
                    {
                        context.Response.StatusCode = badRequestEx.StatusCode;
                    }

                    if (Accepts(context.Request, new MediaTypeHeaderValue("application/json"))
                        && context.RequestServices.GetRequiredService<IProblemDetailsService>() is
                            { } problemDetailsService)
                    {
                        // Write as JSON problem details
                        await problemDetailsService.WriteAsync(new ProblemDetailsContext
                        {
                            HttpContext = context,
                            AdditionalMetadata = exceptionHandlerFeature?.Endpoint?.Metadata,
                            ProblemDetails = { Status = context.Response.StatusCode }
                        });
                    }
                    else
                    {
                        context.Response.ContentType = "text/plain";
                        var message = ReasonPhrases.GetReasonPhrase(context.Response.StatusCode) switch
                        {
                            { Length: > 0 } reasonPhrase => reasonPhrase,
                            _ => "An error occurred"
                        };
                        await context.Response.WriteAsync(message + "\r\n");
                        await context.Response.WriteAsync(
                            $"Request ID: {Activity.Current?.Id ?? context.TraceIdentifier}");
                    }
                }
            });
        }

        return app;
    }

    /// <summary>
    ///     Determines if the request accepts responses formatted as the specified media type via the <c>Accepts</c> header.
    ///     From:
    ///     https://github.com/DamianEdwards/MinimalApiPlayground/blob/main/src/MinimalApiPlayground/Properties/HttpContextExtensions.cs#L25
    /// </summary>
    /// <param name="httpRequest">The <see cref="HttpRequest" />.</param>
    /// <param name="mediaType">The <see cref="MediaTypeHeaderValue" />.</param>
    /// <returns><c>true</c> if the <c>Accept</c> header contains a compatible media type.</returns>
    private static bool Accepts(HttpRequest httpRequest, MediaTypeHeaderValue mediaType)
    {
        if (httpRequest.GetTypedHeaders().Accept is { Count: > 0 } acceptHeader)
        {
            return acceptHeader.Any(mediaType.IsSubsetOf);
        }

        return false;
    }
}