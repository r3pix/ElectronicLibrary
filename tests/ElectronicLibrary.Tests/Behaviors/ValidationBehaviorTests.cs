using ElectronicLibrary.Application.Behaviors;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using NSubstitute;
using Xunit;

namespace ElectronicLibrary.Tests.Behaviors;

public class ValidationBehaviorTests
{
    public class TestRequest : IRequest<string>
    {
        public string Value { get; set; } = string.Empty;
    }

    [Fact]
    public async Task Handle_WhenNoValidatorsRegistered_CallsNext()
    {
        var behavior = new ValidationBehavior<TestRequest, string>([]);
        RequestHandlerDelegate<string> next = _ => Task.FromResult("ok");

        var result = await behavior.Handle(new TestRequest(), next, CancellationToken.None);

        result.Should().Be("ok");
    }

    [Fact]
    public async Task Handle_WhenValidatorsPass_CallsNext()
    {
        var validator = Substitute.For<IValidator<TestRequest>>();
        validator.Validate(Arg.Any<ValidationContext<TestRequest>>()).Returns(new ValidationResult());
        var behavior = new ValidationBehavior<TestRequest, string>([validator]);
        RequestHandlerDelegate<string> next = _ => Task.FromResult("ok");

        var result = await behavior.Handle(new TestRequest(), next, CancellationToken.None);

        result.Should().Be("ok");
    }

    [Fact]
    public async Task Handle_WhenValidatorFails_ThrowsValidationException_AndDoesNotCallNext()
    {
        var failure = new ValidationFailure(nameof(TestRequest.Value), "Value is required");
        var validator = Substitute.For<IValidator<TestRequest>>();
        validator.Validate(Arg.Any<ValidationContext<TestRequest>>()).Returns(new ValidationResult([failure]));
        var behavior = new ValidationBehavior<TestRequest, string>([validator]);
        var nextCalled = false;
        RequestHandlerDelegate<string> next = _ =>
        {
            nextCalled = true;
            return Task.FromResult("ok");
        };

        var act = () => behavior.Handle(new TestRequest(), next, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
        nextCalled.Should().BeFalse();
    }
}
