using System.Text.Json;
using Bogus;
using FluentAssertions;
using OCB.Mediator.Helper.ResultPattern;

namespace OCB.Mediator.Helper.Tests.ResultPattern;

public sealed class OptionalTests
{
    private static readonly JsonSerializerOptions _camelCaseOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly Faker _faker;

    public OptionalTests()
        => _faker = new();

    [Fact]
    public void Some_Should_ReturnHasValueTrue_WhenValueProvided()
    {
        // arrange
        string value = _faker.Commerce.ProductName();

        // act
        Optional<string> optional = Optional<string>.Some(value);

        // assert
        optional.HasValue.Should().BeTrue();
        optional.Value.Should().Be(value);
    }

    [Fact]
    public void Some_Should_ThrowArgumentNullException_WhenValueIsNull()
    {
        // arrange
        string? value = null;

        // act
        Action act = () => Optional<string>.Some(value!);

        // assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void None_Should_ReturnHasValueFalse()
    {
        // act
        Optional<string> optional = Optional<string>.None();

        // assert
        optional.HasValue.Should().BeFalse();
        optional.Value.Should().BeNull();
    }

    [Fact]
    public void NonGenericSome_Should_ReturnHasValueTrue_WhenValueProvided()
    {
        // arrange
        string value = _faker.Commerce.ProductName();

        // act
        Optional<string> optional = Optional.Some(value);

        // assert
        optional.HasValue.Should().BeTrue();
        optional.Value.Should().Be(value);
    }

    [Fact]
    public void ImplicitConversion_Should_ReturnSome_WhenValueProvided()
    {
        // arrange
        string value = _faker.Commerce.ProductName();

        // act
        Optional<string> optional = value;

        // assert
        optional.HasValue.Should().BeTrue();
        optional.Value.Should().Be(value);
    }

    [Fact]
    public void ImplicitConversion_Should_ReturnNone_WhenUsingNoneMarker()
    {
        // act
        Optional<string> optional = Optional.None();

        // assert
        optional.HasValue.Should().BeFalse();
        optional.Value.Should().BeNull();
    }

    [Fact]
    public void Serialize_Should_ProduceHasValueTrueShape_WhenSome()
    {
        // arrange
        string value = _faker.Commerce.ProductName();
        Optional<string> optional = Optional.Some(value);

        // act
        string json = JsonSerializer.Serialize(optional, _camelCaseOptions);

        // assert
        json.Should().Be($$"""{"hasValue":true,"value":{{JsonSerializer.Serialize(value)}}}""");
    }

    [Fact]
    public void Serialize_Should_ProduceHasValueFalseShape_WhenNone()
    {
        // arrange
        Optional<string> optional = Optional<string>.None();

        // act
        string json = JsonSerializer.Serialize(optional, _camelCaseOptions);

        // assert
        json.Should().Be("""{"hasValue":false,"value":null}""");
    }
}
