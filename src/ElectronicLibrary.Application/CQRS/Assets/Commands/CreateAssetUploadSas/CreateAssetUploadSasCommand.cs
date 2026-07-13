using ElectronicLibrary.Application.Models;
using ElectronicLibrary.Domain.Enums;
using ElectronicLibrary.Domain.Models;
using MediatR;

namespace ElectronicLibrary.Application.CQRS.Assets.Commands.CreateAssetUploadSas;

public class CreateAssetUploadSasCommand : IRequest<Response<AssetUploadSasModel>>
{
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public AssetType Type { get; set; }

    public static CreateAssetUploadSasCommand Create(CreateAssetUploadSasRequest dto) =>
        new()
        {
            FileName = dto.FileName,
            ContentType = dto.ContentType,
            Type = dto.Type
        };
}
