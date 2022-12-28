using FluentValidation.Internal;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace Hushify.Api.Resolvers;

public sealed class CamelCasePropertyNameResolver
{
    public static string? ResolvePropertyName(Type type, MemberInfo memberInfo, LambdaExpression expression) =>
        ToCamelCase(DefaultPropertyNameResolver(type, memberInfo, expression));

    private static string? DefaultPropertyNameResolver(Type type, MemberInfo memberInfo, LambdaExpression expression)
    {
        if (expression == null)
        {
            return memberInfo is null ? null : memberInfo.Name;
        }

        var chain = PropertyChain.FromExpression(expression);
        if (chain.Count > 0)
        {
            return chain.ToString();
        }

        return memberInfo is null ? null : memberInfo.Name;
    }

    private static string? ToCamelCase(string? s)
    {
        if (string.IsNullOrWhiteSpace(s) || !char.IsUpper(s[0]))
        {
            return s;
        }

        var chars = s.ToCharArray();

        for (var i = 0; i < chars.Length; i++)
        {
            if (i == 1 && !char.IsUpper(chars[i]))
            {
                break;
            }

            var hasNext = i + 1 < chars.Length;
            if (i > 0 && hasNext && !char.IsUpper(chars[i + 1]))
            {
                break;
            }

            chars[i] = char.ToLower(chars[i], CultureInfo.InvariantCulture);
        }

        return new string(chars);
    }
}