using Hushify.Api.Persistence.Entities;
using Microsoft.AspNetCore.Identity;

namespace Hushify.Api.Persistence.Validators;

public sealed class WorkspaceRoleValidator : IRoleValidator<AppRole>
{
    private readonly IdentityErrorDescriber _describer = new();

    public async Task<IdentityResult> ValidateAsync(RoleManager<AppRole> manager, AppRole role)
    {
        _ = manager ?? throw new ArgumentNullException(nameof(manager));
        _ = role ?? throw new ArgumentNullException(nameof(role));

        var roleName = await manager.GetRoleNameAsync(role);
        if (string.IsNullOrWhiteSpace(roleName))
        {
            return IdentityResult.Failed(_describer.InvalidRoleName(roleName));
        }

        var owner = await manager.FindByNameAsync(roleName);
        if (owner != null && owner.WorkspaceId == role.WorkspaceId && !Equals(owner.Id, role.Id))
        {
            return IdentityResult.Failed(_describer.DuplicateRoleName(roleName));
        }

        return IdentityResult.Success;
    }
}