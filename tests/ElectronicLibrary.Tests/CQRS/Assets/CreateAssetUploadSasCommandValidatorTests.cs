using ElectronicLibrary.Application.CQRS.Assets.Commands.CreateAssetUploadSas;
using ElectronicLibrary.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace ElectronicLibrary.Tests.CQRS.Assets;

public class CreateAssetUploadSasCommandValidatorTests
{
    private readonly CreateAssetUploadSasCommandValidator _validator = new();

    [Fact]
    public void Validate_WhenFileNameEmpty_HasError()
    {
        var command = new CreateAssetUploadSasCommand { FileName = "", ContentType = "application/pdf", Type = AssetType.Score };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateAssetUploadSasCommand.FileName));
    }

    [Fact]
    public void Validate_WhenContentTypeEmpty_HasError()
    {
        var command = new CreateAssetUploadSasCommand { FileName = "example.pdf", ContentType = "", Type = AssetType.Score };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateAssetUploadSasCommand.ContentType));
    }

    [Fact]
    public void Validate_WhenTypeNotInEnum_HasError()
    {
        var command = new CreateAssetUploadSasCommand
        {
            FileName = "example.pdf",
            ContentType = "application/pdf",
            Type = (AssetType)999
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateAssetUploadSasCommand.Type));
    }

    [Fact]
    public void Validate_WhenValid_HasNoErrors()
    {
        var command = new CreateAssetUploadSasCommand
        {
            FileName = "example.pdf",
            ContentType = "application/pdf",
            Type = AssetType.Score
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }
}
