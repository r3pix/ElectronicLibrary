using ElectronicLibrary.Application.CQRS.Assets.Queries.GetAssetDownloadSas;
using ElectronicLibrary.Application.Interfaces;
using ElectronicLibrary.Domain.Entities;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace ElectronicLibrary.Tests.CQRS.Assets;

public class GetAssetDownloadSasQueryHandlerTests
{
    private readonly IAssetRepository _assets = Substitute.For<IAssetRepository>();
    private readonly IBlobStorageService _blobStorage = Substitute.For<IBlobStorageService>();

    [Fact]
    public async Task Handle_WhenAssetMissing_Throws()
    {
        var id = Guid.NewGuid();
        _assets.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((Asset?)null);

        var handler = new GetAssetDownloadSasQueryHandler(_assets, _blobStorage);

        var act = () => handler.Handle(new GetAssetDownloadSasQuery { Id = id }, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenTitleSet_UsesTitleAsFileName()
    {
        var id = Guid.NewGuid();
        var asset = new Asset { Id = id, BlobName = "scores/abc123.pdf", Title = "Original Name.pdf" };
        _assets.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(asset);
        _blobStorage.GetDownloadSasAsync(asset.BlobName, "Original Name.pdf", Arg.Any<CancellationToken>())
            .Returns("https://blob.example/download?sas");

        var handler = new GetAssetDownloadSasQueryHandler(_assets, _blobStorage);

        var response = await handler.Handle(new GetAssetDownloadSasQuery { Id = id }, CancellationToken.None);

        response.Result.Should().Be("https://blob.example/download?sas");
        await _blobStorage.Received(1).GetDownloadSasAsync(asset.BlobName, "Original Name.pdf", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenTitleEmpty_FallsBackToBlobName()
    {
        var id = Guid.NewGuid();
        var asset = new Asset { Id = id, BlobName = "scores/abc123.pdf", Title = string.Empty };
        _assets.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(asset);
        _blobStorage.GetDownloadSasAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("https://blob.example/download?sas");

        var handler = new GetAssetDownloadSasQueryHandler(_assets, _blobStorage);

        await handler.Handle(new GetAssetDownloadSasQuery { Id = id }, CancellationToken.None);

        await _blobStorage.Received(1).GetDownloadSasAsync(asset.BlobName, asset.BlobName, Arg.Any<CancellationToken>());
    }
}
