using AutoMapper;
using ElectronicLibrary.Application.CQRS.Assets.Queries.GetAssets;
using ElectronicLibrary.Application.Interfaces;
using ElectronicLibrary.Application.Profiles;
using ElectronicLibrary.Domain.Entities;
using ElectronicLibrary.Domain.Enums;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace ElectronicLibrary.Tests.CQRS.Assets;

public class GetAssetsQueryHandlerTests
{
    private readonly IAssetRepository _assets = Substitute.For<IAssetRepository>();
    private readonly IBlobStorageService _blobStorage = Substitute.For<IBlobStorageService>();
    private readonly IMapper _mapper;

    public GetAssetsQueryHandlerTests()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<AssetProfile>(), NullLoggerFactory.Instance);
        _mapper = config.CreateMapper();
    }

    [Fact]
    public async Task Handle_PassesRequestedTypeThrough_AndReturnsMappedModels()
    {
        var entities = new List<Asset>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Type = AssetType.Score,
                Status = AssetStatus.Ready,
                BlobName = "scores/example.pdf",
                Title = "example.pdf"
            }
        };
        _assets.GetByTypeAsync(AssetType.Score, Arg.Any<CancellationToken>()).Returns(entities);

        var handler = new GetAssetsQueryHandler(_assets, _blobStorage, _mapper);

        var response = await handler.Handle(new GetAssetsQuery { Type = AssetType.Score }, CancellationToken.None);

        response.IsError.Should().BeFalse();
        response.Result.Should().ContainSingle();
        response.Result[0].BlobName.Should().Be(entities[0].BlobName);
        await _assets.Received(1).GetByTypeAsync(AssetType.Score, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenThumbnailBlobNamePresent_PopulatesThumbnailUrl()
    {
        var asset = new Asset
        {
            Id = Guid.NewGuid(),
            Type = AssetType.Score,
            Status = AssetStatus.Ready,
            BlobName = "scores/example.pdf",
            ThumbnailBlobName = "thumbnails/example.png"
        };
        _assets.GetByTypeAsync(null, Arg.Any<CancellationToken>()).Returns([asset]);
        _blobStorage.GetThumbnailSasAsync("thumbnails/example.png", Arg.Any<CancellationToken>())
            .Returns("https://blob.example/thumbnails/example.png?sas");

        var handler = new GetAssetsQueryHandler(_assets, _blobStorage, _mapper);

        var response = await handler.Handle(new GetAssetsQuery(), CancellationToken.None);

        response.Result[0].ThumbnailUrl.Should().Be("https://blob.example/thumbnails/example.png?sas");
    }

    [Fact]
    public async Task Handle_WhenThumbnailBlobNameAbsent_DoesNotCallBlobStorage()
    {
        var asset = new Asset
        {
            Id = Guid.NewGuid(),
            Type = AssetType.Diploma,
            Status = AssetStatus.Ready,
            BlobName = "diplomas/example.pdf",
            ThumbnailBlobName = null
        };
        _assets.GetByTypeAsync(null, Arg.Any<CancellationToken>()).Returns([asset]);

        var handler = new GetAssetsQueryHandler(_assets, _blobStorage, _mapper);

        var response = await handler.Handle(new GetAssetsQuery(), CancellationToken.None);

        response.Result[0].ThumbnailUrl.Should().BeNull();
        await _blobStorage.DidNotReceive().GetThumbnailSasAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}
