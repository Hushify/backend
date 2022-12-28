using Microsoft.IdentityModel.JsonWebTokens;

namespace Hushify.Api.Constants;

public static class AppClaimTypes
{
    public const string Sub = JwtRegisteredClaimNames.Sub;
    public const string Jti = JwtRegisteredClaimNames.Jti;
    public const string Name = JwtRegisteredClaimNames.Name;
    public const string Role = "role";
    public const string Workspace = "workspace";
}