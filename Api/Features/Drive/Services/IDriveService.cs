using Hushify.Api.Features.Drive.Endpoints;
using Hushify.Api.Features.Drive.Entities;
using Hushify.Api.Features.Identity.Entities;

namespace Hushify.Api.Features.Drive.Services;

public interface IDriveService
{
    Task<ListResponse> ListAsync(Guid? currentFolderId, CancellationToken cancellationToken);
    Task<(long Total, long Used)> GetDriveStatsAsync(CancellationToken cancellationToken);

    Task<CreateMultipartUploadResponse> PrepareForMultipartUploadAsync(Guid parentFolderId, Guid? previousVersionId,
        int numberOfChunks,
        long encryptedSize,
        SecretKeyBundle fileKeyBundle, MetadataBundle metadataBundle, CancellationToken cancellationToken);
}