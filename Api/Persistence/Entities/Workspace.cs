using Hushify.Api.Persistence.Filters;

namespace Hushify.Api.Persistence.Entities;

public sealed class Workspace : ISkipWorkspaceFilter
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public AppUser AppUser { get; set; } = default!;

    public ICollection<AppRole> AppRoles { get; set; } = new HashSet<AppRole>();

    /// <summary>
    ///     Defaults to 1 GB for free accounts
    /// </summary>
    public long StorageSize { get; set; } = (long) 1073741824 * 1;
}