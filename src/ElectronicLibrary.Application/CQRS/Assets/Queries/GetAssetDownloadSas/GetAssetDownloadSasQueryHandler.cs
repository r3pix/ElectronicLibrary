using ElectronicLibrary.Application.Interfaces;
using ElectronicLibrary.Domain.Models;
using MediatR;

namespace ElectronicLibrary.Application.CQRS.Assets.Queries.GetAssetDownloadSas;

public class GetAssetDownloadSasQueryHandler(IAssetRepository assets, IBlobStorageService blobStorage)
    : IRequestHandler<GetAssetDownloadSasQuery, Response<string>>
{
    public async Task<Response<string>> Handle(GetAssetDownloadSasQuery request, CancellationToken ct)
    {
        var entity = await assets.GetByIdAsync(request.Id, ct)
            ?? throw new KeyNotFoundException($"Asset '{request.Id}' was not found.");
        var fileName = string.IsNullOrEmpty(entity.Title) ? entity.BlobName : entity.Title;
        var url = await blobStorage.GetDownloadSasAsync(entity.BlobName, fileName, ct);
        return new Response<string>(url);
    }
}
