using ElectronicLibrary.Domain.Common;
using ElectronicLibrary.Domain.Enums;

namespace ElectronicLibrary.Domain.Entities;

public class Asset : BaseEntity<Guid>
{
    public AssetType Type { get; set; }
    public AssetStatus Status { get; set; }
    public string BlobName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public string UploadedBy { get; set; } = string.Empty;
    public string? ThumbnailBlobName { get; set; }
}
