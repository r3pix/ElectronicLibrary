using ElectronicLibrary.Domain.Enums;

namespace ElectronicLibrary.Application.Models;

public class AssetModel
{
    public Guid Id { get; set; }
    public AssetType Type { get; set; }
    public AssetStatus Status { get; set; }
    public string BlobName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public string UploadedBy { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
}
