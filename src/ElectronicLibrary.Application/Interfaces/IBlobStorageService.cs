using ElectronicLibrary.Application.Models;
using ElectronicLibrary.Domain.Enums;

namespace ElectronicLibrary.Application.Interfaces;

public interface IBlobStorageService
{
    Task<AssetUploadSasModel> GetUploadSasAsync(string fileName, AssetType type, CancellationToken ct = default);
    Task<string> GetDownloadSasAsync(string blobName, string fileName, CancellationToken ct = default);
    Task<string> GetThumbnailSasAsync(string thumbnailBlobName, CancellationToken ct = default);
}
