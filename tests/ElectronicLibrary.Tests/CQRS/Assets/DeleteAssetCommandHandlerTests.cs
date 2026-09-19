using ElectronicLibrary.Application.CQRS.Assets.Commands.DeleteAsset;
using ElectronicLibrary.Application.Interfaces;
using ElectronicLibrary.Domain.Entities;
using NSubstitute;
using Xunit;

namespace ElectronicLibrary.Tests.CQRS.Assets;

public class DeleteAssetCommandHandlerTests
{
    private readonly IAssetRepository _assets = Substitute.For<IAssetRepository>();

    [Fact]
    public async Task Handle_WhenAssetExists_DeletesIt()
    {
        var id = Guid.NewGuid();
        var asset = new Asset { Id = id, BlobName = "scores/example.pdf" };
        _assets.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(asset);

        var handler = new DeleteAssetCommandHandler(_assets);

        await handler.Handle(new DeleteAssetCommand { Id = id }, CancellationToken.None);

        await _assets.Received(1).DeleteAsync(asset, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenAssetMissing_DoesNotCallDelete()
    {
        var id = Guid.NewGuid();
        _assets.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((Asset?)null);

        var handler = new DeleteAssetCommandHandler(_assets);

        await handler.Handle(new DeleteAssetCommand { Id = id }, CancellationToken.None);

        await _assets.DidNotReceive().DeleteAsync(Arg.Any<Asset>(), Arg.Any<CancellationToken>());
    }
}
