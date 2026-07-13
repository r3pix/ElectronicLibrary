using MediatR;

namespace ElectronicLibrary.Application.CQRS.Assets.Commands.DeleteAsset;

public class DeleteAssetCommand : IRequest
{
    public Guid Id { get; set; }
}
