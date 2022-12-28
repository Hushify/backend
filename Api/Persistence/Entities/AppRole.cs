using Hushify.Api.Persistence.Filters;
using Microsoft.AspNetCore.Identity;

namespace Hushify.Api.Persistence.Entities;

public sealed class AppRole : IdentityRole<Guid>, IWorkspaceFilter
{
    public Workspace Workspace { get; set; } = default!;
    public Guid WorkspaceId { get; set; }
}