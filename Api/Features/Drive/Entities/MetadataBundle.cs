using Hushify.Api.Filters;

namespace Hushify.Api.Features.Drive.Entities;

public sealed record MetadataBundle(string Nonce, string Metadata) : ISkipWorkspaceFilter;