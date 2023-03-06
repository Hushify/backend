using Hushify.Api.Filters;

namespace Hushify.Api.Features.Drive.Entities;

public sealed class FileS3Config : ISkipWorkspaceFilter
{
    public string Region { get; set; } = default!;
    public string BucketName { get; set; } = default!;
    public string Key { get; set; } = default!;
    public string UploadId { get; set; } = default!;
}