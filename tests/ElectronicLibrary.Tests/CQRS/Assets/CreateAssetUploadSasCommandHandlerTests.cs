using ElectronicLibrary.Application.CQRS.Assets.Commands.CreateAssetUploadSas;
using ElectronicLibrary.Application.Interfaces;
using ElectronicLibrary.Application.Models;
using ElectronicLibrary.Domain.Entities;
using ElectronicLibrary.Domain.Enums;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace ElectronicLibrary.Tests.CQRS.Assets;

public class CreateAssetUploadSasCommandHandlerTests
{
    private readonly IAssetRepository _assets = Substitute.For<IAssetRepository>();
    private readonly IBlobStorageService _blobStorage = Substitute.For<IBlobStorageService>();
    private readonly ICurrentUserService _currentUser = Substitute.For<ICurrentUserService>();

    [Fact]
    public async Task Handle_CreatesPendingAssetStub_AndReturnsSas()
    {
        var sas = new AssetUploadSasModel { UploadUrl = "https://blob.example/upload?sas", BlobName = "scores/abc123.pdf" };
        _blobStorage.GetUploadSasAsync("example.pdf", AssetType.Score, Arg.Any<CancellationToken>()).Returns(sas);
        _currentUser.Email.Returns("user@example.com");

        var handler = new CreateAssetUploadSasCommandHandler(_assets, _blobStorage, _currentUser);
        var command = new CreateAssetUploadSasCommand
        {
            FileName = "example.pdf",
            ContentType = "application/pdf",
            Type = AssetType.Score
        };

        var response = await handler.Handle(command, CancellationToken.None);

        response.IsError.Should().BeFalse();
        response.Result.Should().Be(sas);
        await _assets.Received(1).AddAsync(
            Arg.Is<Asset>(a =>
                a != null &&
                a.Status == AssetStatus.Pending &&
                a.Type == AssetType.Score &&
                a.BlobName == sas.BlobName &&
                a.Title == "example.pdf" &&
                a.UploadedBy == "user@example.com"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenCurrentUserEmailIsNull_UsesEmptyString()
    {
        var sas = new AssetUploadSasModel { UploadUrl = "https://blob.example/upload?sas", BlobName = "scores/abc123.pdf" };
        _blobStorage.GetUploadSasAsync(Arg.Any<string>(), Arg.Any<AssetType>(), Arg.Any<CancellationToken>()).Returns(sas);
        _currentUser.Email.Returns((string?)null);

        var handler = new CreateAssetUploadSasCommandHandler(_assets, _blobStorage, _currentUser);
        var command = new CreateAssetUploadSasCommand
        {
            FileName = "example.pdf",
            ContentType = "application/pdf",
            Type = AssetType.Score
        };

        await handler.Handle(command, CancellationToken.None);

        await _assets.Received(1).AddAsync(Arg.Is<Asset>(a => a != null && a.UploadedBy == string.Empty), Arg.Any<CancellationToken>());
    }
}
