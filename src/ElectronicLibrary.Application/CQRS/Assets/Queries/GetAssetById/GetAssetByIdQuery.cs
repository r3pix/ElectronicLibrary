using ElectronicLibrary.Application.Models;
using ElectronicLibrary.Domain.Models;
using MediatR;

namespace ElectronicLibrary.Application.CQRS.Assets.Queries.GetAssetById;

public class GetAssetByIdQuery : IRequest<Response<AssetModel>>
{
    public Guid Id { get; set; }
}
