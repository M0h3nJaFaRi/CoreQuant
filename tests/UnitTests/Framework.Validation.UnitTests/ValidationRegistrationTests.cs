using FluentValidation;
using Framework.Validation;
using Microsoft.Extensions.DependencyInjection;

namespace Framework.Validation.UnitTests;

public sealed class ValidationRegistrationTests
{
    [Fact]
    public void AddFrameworkValidation_Registers_Validators_From_Assembly()
    {
        var services = new ServiceCollection();
        services.AddFrameworkValidation(typeof(ValidationRegistrationTests).Assembly);
        using var provider = services.BuildServiceProvider();

        var validator = provider.GetRequiredService<IValidator<SampleInput>>();
        var result = validator.Validate(new SampleInput(""));

        Assert.False(result.IsValid);
    }

    [Fact]
    public void AddFrameworkValidation_Throws_When_No_Assemblies()
    {
        var services = new ServiceCollection();
        Assert.Throws<ArgumentException>(() => services.AddFrameworkValidation());
    }

    public sealed record SampleInput(string Name);

    public sealed class SampleInputValidator : AbstractValidator<SampleInput>
    {
        public SampleInputValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
        }
    }
}
