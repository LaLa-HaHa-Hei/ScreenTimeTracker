using System.Text.Json.Serialization;
using ScreenTimeTracker.BuildingBlocks.Types;

namespace ScreenTimeTracker.ScreenTime.Features.AppCategories.PatchAppCategory;

public record PatchAppCategoryRequest(
    Guid AppCategoryId,
    [property: JsonConverter(typeof(OptionalValueJsonConverter<string>))]
        OptionalValue<string> Name,
    [property: JsonConverter(typeof(OptionalValueJsonConverter<string>))]
        OptionalValue<string> Color,
    [property: JsonConverter(typeof(OptionalValueJsonConverter<string?>))]
        OptionalValue<string?> IconPath
);
