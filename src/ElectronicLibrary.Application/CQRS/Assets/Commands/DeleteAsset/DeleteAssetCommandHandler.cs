using ElectronicLibrary.Application.Interfaces;
using MediatR;

namespace ElectronicLibrary.Application.CQRS.Assets.Commands.DeleteAsset;

public class DeleteAssetCommandHandler(IAssetRepository assets) : IRequestHandler<DeleteAssetCommand>
{
    public async Task Handle(DeleteAssetCommand request, CancellationToken ct)
    {
        var entity = await assets.GetByIdAsync(request.Id, ct);
        if (entity is not null)
            await assets.DeleteAsync(entity, ct);
    }
}
