namespace Sheared.Models.ResponseModels;

public sealed record ProductImageDto(Guid MediaFileId, string Uri, int SortOrder);
