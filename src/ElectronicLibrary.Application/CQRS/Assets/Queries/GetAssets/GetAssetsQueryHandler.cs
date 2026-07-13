using AutoMapper;
using ElectronicLibrary.Application.Interfaces;
using ElectronicLibrary.Application.Models;
using ElectronicLibrary.Domain.Models;
using MediatR;

namespace ElectronicLibrary.Application.CQRS.Assets.Queries.GetAssets;

public class GetAssetsQueryHandler(IAssetRepository assets, IBlobStorageService blobStorage, IMapper mapper)
    : IRequestHandler<GetAssetsQuery, Response<List<AssetModel>>>
{
    public async Task<Response<List<AssetModel>>> Handle(GetAssetsQuery request, CancellationToken ct)
    {
        var entities = await assets.GetByTypeAsync(request.Type, ct);
        var models = mapper.Map<List<AssetModel>>(entities);

        for (var i = 0; i < entities.Count; i++)
        {
            if (entities[i].ThumbnailBlobName is { } thumbnailBlobName)
                models[i].ThumbnailUrl = await blobStorage.GetThumbnailSasAsync(thumbnailBlobName, ct);
        }

        return new Response<List<AssetModel>>(models);
    }
}
