using Application.Common.Behaviours;
using FluentValidation;
using MediatR;

namespace OrderFlowII.Tests;

public class ValidationBehaviorTests
{
    // request صغير للتست بس، مش محتاجين CreateOrderCommand
    private  sealed  record TestRequest(string Name) : IRequest<string>;

    [Fact]
    public async Task Handle_WhenNoValidators_CallsNextAndReturnsItsResult()
    {
        // Arrange
        var behavior = new ValidationBehavior<TestRequest, string>(new List<IValidator<TestRequest>>());

        var nextCalled = false;
        RequestHandlerDelegate<string> next = () =>
        {
            nextCalled = true;
            return Task.FromResult("handler result");
        };

        // Act
        var result = await behavior.Handle(new TestRequest(""), next, CancellationToken.None);

        // Assert
        Assert.True(nextCalled);
        Assert.Equal("handler result", result);
    }

    [Fact]
    public async Task Handle_WhenRequestIsValid_CallsNextAndReturnsItsResult()
    {
        // Arrange
        var validator = new InlineValidator<TestRequest>();
        validator.RuleFor(x => x.Name).NotEmpty();

        var behavior = new ValidationBehavior<TestRequest, string>(new[] { validator });

        var nextCalled = false;
        RequestHandlerDelegate<string> next = () =>
        {
            nextCalled = true;
            return Task.FromResult("handler result");
        };

        // Act
        var result = await behavior.Handle(new TestRequest("Ahmed"), next, CancellationToken.None);

        // Assert
        Assert.True(nextCalled);
        Assert.Equal("handler result", result);
    }

    [Fact]
    public async Task Handle_WhenRequestIsInvalid_ThrowsValidationExceptionAndDoesNotCallNext()
    {
        // Arrange
        var validator = new InlineValidator<TestRequest>();
        validator.RuleFor(x => x.Name).NotEmpty();

        var behavior = new ValidationBehavior<TestRequest, string>(new[] { validator });

        var nextCalled = false;
        RequestHandlerDelegate<string> next = () =>
        {
            nextCalled = true;
            return Task.FromResult("handler result");
        };

        // Act
        var ex = await Assert.ThrowsAsync<ValidationException>(
            () => behavior.Handle(new TestRequest(""), next, CancellationToken.None));

        // Assert
        Assert.False(nextCalled);                                      // الـ handler مااتنداش
        Assert.Contains(ex.Errors, e => e.PropertyName == "Name");     // الـ error على الـ property الصح
    }

    [Fact]
    public async Task Handle_WhenMultipleValidatorsFail_CombinesAllErrors()
    {
        // Arrange: اتنين validators، كل واحد بيفشل بـ error
        var validator1 = new InlineValidator<TestRequest>();
        validator1.RuleFor(x => x.Name).NotEmpty();

        var validator2 = new InlineValidator<TestRequest>();
        validator2.RuleFor(x => x.Name).MinimumLength(3);

        var behavior = new ValidationBehavior<TestRequest, string>(new[] { validator1, validator2 });

        RequestHandlerDelegate<string> next = () => Task.FromResult("handler result");

        // Act
        var ex = await Assert.ThrowsAsync<ValidationException>(
            () => behavior.Handle(new TestRequest(""), next, CancellationToken.None));

        // Assert
        Assert.Equal(2, ex.Errors.Count());
    }
}
