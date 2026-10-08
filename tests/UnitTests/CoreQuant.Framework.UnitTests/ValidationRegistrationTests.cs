using FluentValidation;
using Framework;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CoreQuant.Framework.UnitTests;

public sealed class ValidationRegistrationTests
{
    [Fact]
    public void AddFramework_Registers_Validators_From_Assembly()
    {
        var services = new ServiceCollection();
        services.AddFramework(
            new ConfigurationBuilder().Build(),
            "test-service",
            typeof(ValidationRegistrationTests).Assembly);
        using var provider = services.BuildServiceProvider();

        var validator = provider.GetRequiredService<IValidator<SampleInput>>();
        var result = validator.Validate(new SampleInput(""));

        Assert.False(result.IsValid);
    }

    [Fact]
    public void AddFramework_Without_Validation_Assemblies_Does_Not_Register_Validators()
    {
        var services = new ServiceCollection();
        services.AddFramework(new ConfigurationBuilder().Build(), "test-service");

        Assert.Throws<InvalidOperationException>(() =>
            services.BuildServiceProvider().GetRequiredService<IValidator<SampleInput>>());
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
