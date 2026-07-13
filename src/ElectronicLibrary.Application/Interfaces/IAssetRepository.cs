using ElectronicLibrary.Domain.Entities;
using ElectronicLibrary.Domain.Enums;

namespace ElectronicLibrary.Application.Interfaces;

public interface IAssetRepository : IRepository<Asset>
{
    Task<Asset?> GetByBlobNameAsync(string blobName, CancellationToken ct = default);
    Task<List<Asset>> GetByTypeAsync(AssetType? type, CancellationToken ct = default);
}
