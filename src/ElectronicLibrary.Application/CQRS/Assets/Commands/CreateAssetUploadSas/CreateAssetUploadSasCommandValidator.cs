using FluentValidation;

namespace ElectronicLibrary.Application.CQRS.Assets.Commands.CreateAssetUploadSas;

public class CreateAssetUploadSasCommandValidator : AbstractValidator<CreateAssetUploadSasCommand>
{
    public CreateAssetUploadSasCommandValidator()
    {
        RuleFor(x => x.FileName).NotEmpty();
        RuleFor(x => x.ContentType).NotEmpty();
        RuleFor(x => x.Type).IsInEnum();
    }
}
