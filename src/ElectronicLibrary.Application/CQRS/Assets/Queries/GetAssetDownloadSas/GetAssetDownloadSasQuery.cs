using ElectronicLibrary.Domain.Models;
using MediatR;

namespace ElectronicLibrary.Application.CQRS.Assets.Queries.GetAssetDownloadSas;

public class GetAssetDownloadSasQuery : IRequest<Response<string>>
{
    public Guid Id { get; set; }
}
