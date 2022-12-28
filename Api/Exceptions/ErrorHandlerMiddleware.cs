using Humanizer;
using Hushify.Api.Constants;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;

namespace Hushify.Api.Exceptions;

public sealed class ErrorHandlerMiddleware
{
    private readonly RequestDelegate _next;

    public ErrorHandlerMiddleware(RequestDelegate next) => _next = next;

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (AppException ex)
        {
            var response = context.Response;
            response.ContentType = "application/problem+json";
            response.StatusCode = (int) HttpStatusCode.BadRequest;

            var errors = new Dictionary<string, string[]>();
            foreach (var error in ex.Failures)
            {
                var key = error.Name?.Camelize() ?? ErrorConstants.Errors.Camelize();
                var currentErrors = errors.GetValueOrDefault(key, new string[1]);
                currentErrors[0] = error.Message;
                errors[key] = currentErrors;
            }

            var problemDetails = new ValidationProblemDetails(errors)
            {
                Detail = ex?.Message ?? string.Empty,
                Status = (int) HttpStatusCode.InternalServerError
            };

            await response.WriteAsync(JsonSerializer.Serialize(problemDetails));
        }
        catch (Exception ex)
        {
            var response = context.Response;
            response.ContentType = "application/problem+json";

            response.StatusCode = ex is KeyNotFoundException
                ? (int) HttpStatusCode.NotFound
                : (int) HttpStatusCode.InternalServerError;

            var problemDetails = new ProblemDetails
            {
                Detail = ex?.Message ?? string.Empty,
                Status = (int) HttpStatusCode.InternalServerError
            };

            await response.WriteAsync(JsonSerializer.Serialize(problemDetails));
        }
    }
}