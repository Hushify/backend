using Hushify.Api.Constants;
using Hushify.Api.Exceptions;

namespace Hushify.Api.Persistence.Providers;

public interface IWorkspaceProvider
{
    public Guid GetWorkspaceId();
}

public sealed class WorkspaceProvider : IWorkspaceProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public WorkspaceProvider(IHttpContextAccessor httpContextAccessor) => _httpContextAccessor = httpContextAccessor;

    public Guid GetWorkspaceId()
    {
        var workspaceClaim =
            _httpContextAccessor?.HttpContext?.User.Claims.FirstOrDefault(c => c.Type == AppClaimTypes.Workspace);

        if (workspaceClaim is null)
        {
            throw new AppException("User doesn't belong to any Workspace.",
                new[] { new AppFailure(ErrorConstants.Errors, "Workspace Id was not found in the user claims.") });
        }

        if (string.IsNullOrWhiteSpace(workspaceClaim.Value) || !Guid.TryParse(workspaceClaim.Value, out var id))
        {
            throw new AppException("Failed to correctly parse the Workspace Id.",
                new[] { new AppFailure(ErrorConstants.Errors, "Workspace Id was not in the correct format.") });
        }

        return id;
    }
}