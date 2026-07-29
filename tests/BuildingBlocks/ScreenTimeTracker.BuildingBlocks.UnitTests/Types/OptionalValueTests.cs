using System.Text.Json;
using ScreenTimeTracker.BuildingBlocks.Types;
using Shouldly;

namespace ScreenTimeTracker.BuildingBlocks.UnitTests.Types;

public class OptionalValueTests
{
    private sealed class TestDto
    {
        public OptionalValue<string?> Name { get; set; }
        public OptionalValue<int> Age { get; set; }
    }

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        Converters = { new OptionalValueJsonConverterFactory() },
        PropertyNameCaseInsensitive = true,
    };

    #region OptionalValue<T> 结构体基础功能测试

    [Fact]
    public void DefaultConstructor_ShouldBeUndefined_AndThrowOnAccessValue()
    {
        // Arrange & Act
        var optional = new OptionalValue<string>();

        // Assert
        optional.HasValue.ShouldBeFalse();

        // 访问未赋值的 Value 应抛出 InvalidOperationException
        var exception = Should.Throw<InvalidOperationException>(() => _ = optional.Value);
        exception.Message.ShouldContain(
            "Property 'Value' cannot be accessed when 'HasValue' is false"
        );
    }

    [Fact]
    public void Constructor_WithValue_ShouldSetHasValueAndValue()
    {
        // Arrange & Act
        var optional = new OptionalValue<string>("hello");

        // Assert
        optional.HasValue.ShouldBeTrue();
        optional.Value.ShouldBe("hello");
    }

    [Fact]
    public void ImplicitOperator_ShouldConvertValueToOptional()
    {
        // Act - 隐式转换
        OptionalValue<int> optional = 42;

        // Assert
        optional.HasValue.ShouldBeTrue();
        optional.Value.ShouldBe(42);
    }

    #endregion

    #region JSON Converter (反序列化与序列化) 测试

    [Fact]
    public void Deserialize_WhenPropertyIsMissingInJson_HasValueShouldBeFalse()
    {
        // Arrange: 缺失 Name 和 Age 字段
        var json = "{}";

        // Act
        var result = JsonSerializer.Deserialize<TestDto>(json, _jsonOptions);

        // Assert
        result.ShouldNotBeNull();
        result.Name.HasValue.ShouldBeFalse();
        result.Age.HasValue.ShouldBeFalse();
    }

    [Fact]
    public void Deserialize_WhenPropertyExistsInJson_HasValueShouldBeTrue()
    {
        // Arrange: 提供了有效的字段
        var json = """{"Name": "Alice", "Age": 20}""";

        // Act
        var result = JsonSerializer.Deserialize<TestDto>(json, _jsonOptions);

        // Assert
        result.ShouldNotBeNull();
        result.Name.HasValue.ShouldBeTrue();
        result.Name.Value.ShouldBe("Alice");

        result.Age.HasValue.ShouldBeTrue();
        result.Age.Value.ShouldBe(20);
    }

    [Fact]
    public void Deserialize_WhenPropertyIsNullInJson_HasValueShouldBeTrueAndValueIsNull()
    {
        // Arrange: 显式传入了 null
        var json = """{"Name": null}""";

        // Act
        var result = JsonSerializer.Deserialize<TestDto>(json, _jsonOptions);

        // Assert
        result.ShouldNotBeNull();
        result.Name.HasValue.ShouldBeTrue();
        result.Name.Value.ShouldBeNull();
    }

    [Fact]
    public void Serialize_ShouldThrowNotSupportedException()
    {
        // Arrange
        var dto = new TestDto { Name = "Bob" };

        // Act & Assert: 序列化时应该抛出 NotSupportedException
        var exception = Should.Throw<NotSupportedException>(() =>
            JsonSerializer.Serialize(dto, _jsonOptions)
        );
        exception.Message.ShouldContain("does not support JSON serialization");
    }

    #endregion
}
