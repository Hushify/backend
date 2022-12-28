using Hushify.Api.Persistence.Filters;

namespace Hushify.Api.Persistence.Entities.Drive;

public sealed record MetadataBundle(string Nonce, string Metadata) : ISkipWorkspaceFilter;