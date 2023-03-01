using Hushify.Api.Filters;

namespace Hushify.Api.Features.Drive.Entities;

public sealed class PartETag : ISkipWorkspaceFilter
{
    private PartETag() { }

    public PartETag(int partNumber, string eTag)
    {
        PartNumber = partNumber;
        ETag = eTag;
    }

    public required int PartNumber { get; set; }
    public required string ETag { get; set; }
}