using ElectronicLibrary.Application.Interfaces;
using ElectronicLibrary.Application.Models;
using ElectronicLibrary.Domain.Entities;
using ElectronicLibrary.Domain.Enums;
using ElectronicLibrary.Domain.Models;
using MediatR;

namespace ElectronicLibrary.Application.CQRS.Assets.Commands.CreateAssetUploadSas;

public class CreateAssetUploadSasCommandHandler(
    IAssetRepository assets,
    IBlobStorageService blobStorage,
    ICurrentUserService currentUser)
    : IRequestHandler<CreateAssetUploadSasCommand, Response<AssetUploadSasModel>>
{
    public async Task<Response<AssetUploadSasModel>> Handle(CreateAssetUploadSasCommand request, CancellationToken ct)
    {
        var sas = await blobStorage.GetUploadSasAsync(request.FileName, request.Type, ct);

        var asset = new Asset
        {
            Type = request.Type,
            Status = AssetStatus.Pending,
            BlobName = sas.BlobName,
            Title = request.FileName,
            UploadedBy = currentUser.Email ?? string.Empty
        };
        await assets.AddAsync(asset, ct);

        return new Response<AssetUploadSasModel>(sas);
    }
}
