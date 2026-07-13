using ElectronicLibrary.Domain.Enums;

namespace ElectronicLibrary.Application.Models;

public class CreateAssetUploadSasRequest
{
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public AssetType Type { get; set; }
}
