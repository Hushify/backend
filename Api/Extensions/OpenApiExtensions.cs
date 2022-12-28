using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;

namespace Hushify.Api.Extensions;

public static class OpenApiExtensions
{
    /// <summary>
    ///     Adds the JWT security scheme to the Open API description
    ///     From: https://github.com/davidfowl/TodoApi/blob/main/TodoApi/Extensions/OpenApiExtensions.cs
    /// </summary>
    /// <param name="builder">
    ///     <see cref="IEndpointConventionBuilder" />
    /// </param>
    /// <returns>
    ///     <see cref="IEndpointConventionBuilder" />
    /// </returns>
    public static IEndpointConventionBuilder AddOpenApiSecurityRequirement(this IEndpointConventionBuilder builder)
    {
        var scheme = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Name = JwtBearerDefaults.AuthenticationScheme,
            Scheme = JwtBearerDefaults.AuthenticationScheme,
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = JwtBearerDefaults.AuthenticationScheme
            }
        };

        return builder.WithOpenApi(operation => new OpenApiOperation(operation)
        {
            Security =
            {
                new OpenApiSecurityRequirement
                {
                    [scheme] = new List<string>()
                }
            }
        });
    }
}