using ElectronicLibrary.Application.Models;
using ElectronicLibrary.Domain.Enums;
using ElectronicLibrary.Domain.Models;
using MediatR;

namespace ElectronicLibrary.Application.CQRS.Assets.Queries.GetAssets;

public class GetAssetsQuery : IRequest<Response<List<AssetModel>>>
{
    public AssetType? Type { get; set; }
}
