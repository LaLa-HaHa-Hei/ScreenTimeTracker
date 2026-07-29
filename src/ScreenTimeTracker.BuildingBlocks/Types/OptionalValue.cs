using System.Text.Json;
using System.Text.Json.Serialization;

namespace ScreenTimeTracker.BuildingBlocks.Types;

/// <summary>
/// Represents an optional value that may be present or undefined.
/// Used primarily to distinguish between <c>null</c> (an explicitly specified null value) and <c>undefined</c> (no value was provided).
/// </summary>
public readonly record struct OptionalValue<T>
{
    private readonly T _value = default!;
    public T Value =>
        HasValue
            ? _value
            : throw new InvalidOperationException(
                "Property 'Value' cannot be accessed when 'HasValue' is false. Check 'HasValue' before accessing the value."
            );
    public bool HasValue { get; init; }

    public OptionalValue(T value)
    {
        _value = value;
        HasValue = true;
    }

    // 隐式转换
    public static implicit operator OptionalValue<T>(T value) => new(value);
}

/// <summary>
/// During deserialization:
/// JSON fields that are missing are mapped to HasValue=false;
/// fields that exist in JSON (even if the value is null) are mapped to HasValue=true.
/// During serialization: Not supported; a NotSupportedException will be thrown.
/// </summary>
public class OptionalValueJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert) =>
        typeToConvert.IsGenericType
        && typeToConvert.GetGenericTypeDefinition() == typeof(OptionalValue<>);

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var valueType = typeToConvert.GetGenericArguments()[0];
        return (JsonConverter)
            Activator.CreateInstance(
                typeof(OptionalValueJsonConverter<>).MakeGenericType(valueType)
            )!;
    }
}

public class OptionalValueJsonConverter<T> : JsonConverter<OptionalValue<T>>
{
    public override bool HandleNull => true;

    public override OptionalValue<T> Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        // 只要 Read 被调用，说明 Key 存在
        // 如果 JSON 值是 null，Deserialize 会返回 default(T)
        var value = JsonSerializer.Deserialize<T>(ref reader, options);
        return new OptionalValue<T>(value!);
    }

    public override void Write(
        Utf8JsonWriter writer,
        OptionalValue<T> value,
        JsonSerializerOptions options
    )
    {
        throw new NotSupportedException(
            $"{nameof(OptionalValue<>)} does not support JSON serialization."
        );
    }
}
