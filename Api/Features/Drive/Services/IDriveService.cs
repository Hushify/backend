using Hushify.Api.Features.Drive.Endpoints;

namespace Hushify.Api.Features.Drive.Services;

public interface IDriveService
{
    Task<ListResponse> ListAsync(Guid? currentFolderId, CancellationToken cancellationToken);
}