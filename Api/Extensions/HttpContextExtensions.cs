namespace Hushify.Api.Extensions;

public static class HttpContextExtensions
{
    public static string GetUserIpAddress(this HttpContext ctx) =>
        ctx.Request.Headers.ContainsKey("X-Forwarded-For")
            ? ctx.Request.Headers["X-Forwarded-For"].ToString()
            : ctx.Connection.RemoteIpAddress?.ToString() ?? string.Empty;

    public static string GetUserAgent(this HttpContext ctx) =>
        ctx.Request.Headers?.UserAgent.ToString() ?? string.Empty;
}