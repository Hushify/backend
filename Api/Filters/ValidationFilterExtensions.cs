using FluentValidation;
using Microsoft.AspNetCore.Http.Metadata;
using System.Net;
using System.Reflection;

namespace Hushify.Api.Filters;

public static class ValidationFilterExtensions
{
    public static TBuilder WithParameterValidation<TBuilder>(this TBuilder builder, params Type[] typesToValidate)
        where TBuilder : IEndpointConventionBuilder
    {
        builder.Add(eb =>
        {
            var methodInfo = eb.Metadata.OfType<MethodInfo>().FirstOrDefault();

            if (methodInfo is null)
            {
                return;
            }

            // Track the indicies of validatable parameters
            var parametersToValidate = methodInfo.GetParameters()
                .Where(p => typesToValidate.Contains(p.ParameterType))
                .Select(p => (index: p.Position, type: p.ParameterType)).ToList();

            if (parametersToValidate.Count <= 0)
            {
                // Nothing to validate so don't add the filter to this endpoint
                return;
            }

            // We can respond with problem details if there's a validation error
            eb.Metadata.Add(new ProducesResponseTypeMetadata(typeof(HttpValidationProblemDetails), 400,
                "application/problem+json"));

            eb.FilterFactories.Add((context, next) =>
            {
                return async efic =>
                {
                    foreach (var (index, type) in parametersToValidate)
                    {
                        if (efic.Arguments[index] is not { } arg)
                        {
                            continue;
                        }

                        var validatorType = typeof(IValidator<>).MakeGenericType(type);
                        if (efic.HttpContext.RequestServices.GetService(validatorType) is not IValidator validator)
                        {
                            continue;
                        }

                        var validationResult = await validator.ValidateAsync(new ValidationContext<object>(arg),
                            efic.HttpContext.RequestAborted);

                        if (validationResult.IsValid)
                        {
                            continue;
                        }

                        efic.HttpContext.Response.StatusCode = (int) HttpStatusCode.BadRequest;
                        return TypedResults.ValidationProblem(validationResult.ToDictionary());
                    }

                    return await next(efic);
                };
            });
        });

        return builder;
    }

    // Equivalent to the .Produces call to add metadata to endpoints
    private sealed class ProducesResponseTypeMetadata : IProducesResponseTypeMetadata
    {
        public ProducesResponseTypeMetadata(Type type, int statusCode, string contentType)
        {
            Type = type;
            StatusCode = statusCode;
            ContentTypes = new[] { contentType };
        }

        public Type Type { get; }
        public int StatusCode { get; }
        public IEnumerable<string> ContentTypes { get; }
    }
}