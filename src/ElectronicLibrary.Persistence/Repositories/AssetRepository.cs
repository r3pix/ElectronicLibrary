using ElectronicLibrary.Application.Interfaces;
using ElectronicLibrary.Domain.Entities;
using ElectronicLibrary.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ElectronicLibrary.Persistence.Repositories;

public class AssetRepository(ApplicationDbContext context) : Repository<Asset>(context), IAssetRepository
{
    public async Task<Asset?> GetByBlobNameAsync(string blobName, CancellationToken ct = default) =>
        await Context.Assets.FirstOrDefaultAsync(a => a.BlobName == blobName, ct);

    public async Task<List<Asset>> GetByTypeAsync(AssetType? type, CancellationToken ct = default)
    {
        var query = Context.Assets.AsQueryable();
        if (type.HasValue)
            query = query.Where(a => a.Type == type.Value);

        return await query.ToListAsync(ct);
    }
}
