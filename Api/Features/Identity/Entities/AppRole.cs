using Hushify.Api.Filters;
using Microsoft.AspNetCore.Identity;

namespace Hushify.Api.Features.Identity.Entities;

public sealed class AppRole : IdentityRole<Guid>, IWorkspaceFilter
{
    public Workspace Workspace { get; set; } = default!;
    public Guid WorkspaceId { get; set; }
}