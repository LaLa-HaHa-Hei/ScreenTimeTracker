using System.Text.Json.Serialization;
using ScreenTimeTracker.BuildingBlocks.Types;

namespace ScreenTimeTracker.ScreenTime.Features.Apps.PatchApp;

public record PatchAppRequest(
    Guid Id,
    [property: JsonConverter(typeof(OptionalValueJsonConverter<string>))]
        OptionalValue<string> Name,
    [property: JsonConverter(typeof(OptionalValueJsonConverter<string>))]
        OptionalValue<string> Color,
    [property: JsonConverter(typeof(OptionalValueJsonConverter<bool>))]
        OptionalValue<bool> AllowMetadataAutoUpdate,
    [property: JsonConverter(typeof(OptionalValueJsonConverter<Guid>))]
        OptionalValue<Guid> AppCategoryId,
    [property: JsonConverter(typeof(OptionalValueJsonConverter<string?>))]
        OptionalValue<string?> IconPath
);
