using AutoMapper;
using ElectronicLibrary.Application.Interfaces;
using ElectronicLibrary.Application.Models;
using ElectronicLibrary.Domain.Models;
using MediatR;

namespace ElectronicLibrary.Application.CQRS.Assets.Queries.GetAssetById;

public class GetAssetByIdQueryHandler(IAssetRepository assets, IBlobStorageService blobStorage, IMapper mapper)
    : IRequestHandler<GetAssetByIdQuery, Response<AssetModel>>
{
    public async Task<Response<AssetModel>> Handle(GetAssetByIdQuery request, CancellationToken ct)
    {
        var entity = await assets.GetByIdAsync(request.Id, ct)
            ?? throw new KeyNotFoundException($"Asset '{request.Id}' was not found.");

        var model = mapper.Map<AssetModel>(entity);
        if (entity.ThumbnailBlobName is { } thumbnailBlobName)
            model.ThumbnailUrl = await blobStorage.GetThumbnailSasAsync(thumbnailBlobName, ct);

        return new Response<AssetModel>(model);
    }
}
