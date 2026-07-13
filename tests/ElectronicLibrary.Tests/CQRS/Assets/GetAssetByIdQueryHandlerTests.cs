using AutoMapper;
using ElectronicLibrary.Application.CQRS.Assets.Queries.GetAssetById;
using ElectronicLibrary.Application.Interfaces;
using ElectronicLibrary.Application.Profiles;
using ElectronicLibrary.Domain.Entities;
using ElectronicLibrary.Domain.Enums;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace ElectronicLibrary.Tests.CQRS.Assets;

public class GetAssetByIdQueryHandlerTests
{
    private readonly IAssetRepository _assets = Substitute.For<IAssetRepository>();
    private readonly IBlobStorageService _blobStorage = Substitute.For<IBlobStorageService>();
    private readonly IMapper _mapper;

    public GetAssetByIdQueryHandlerTests()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<AssetProfile>(), NullLoggerFactory.Instance);
        _mapper = config.CreateMapper();
    }

    [Fact]
    public async Task Handle_WhenAssetExists_ReturnsMappedModel()
    {
        var id = Guid.NewGuid();
        var asset = new Asset
        {
            Id = id,
            Type = AssetType.Score,
            Status = AssetStatus.Ready,
            BlobName = "scores/example.pdf",
            Title = "example.pdf",
            ContentType = "application/pdf",
            SizeBytes = 1024,
            UploadedBy = "user@example.com"
        };
        _assets.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(asset);

        var handler = new GetAssetByIdQueryHandler(_assets, _blobStorage, _mapper);

        var response = await handler.Handle(new GetAssetByIdQuery { Id = id }, CancellationToken.None);

        response.IsError.Should().BeFalse();
        response.Result.Id.Should().Be(id);
        response.Result.BlobName.Should().Be(asset.BlobName);
    }

    [Fact]
    public async Task Handle_WhenAssetMissing_Throws()
    {
        var id = Guid.NewGuid();
        _assets.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((Asset?)null);

        var handler = new GetAssetByIdQueryHandler(_assets, _blobStorage, _mapper);

        var act = () => handler.Handle(new GetAssetByIdQuery { Id = id }, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
