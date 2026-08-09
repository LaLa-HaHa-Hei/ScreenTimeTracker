using FastEndpoints;
using FluentValidation;
using ScreenTimeTracker.BuildingBlocks.Types;

namespace ScreenTimeTracker.ScreenTime.Features.UserSettingsManagement.PatchUserSettings;

public class PatchUserSettingsRequestValidator : Validator<PatchUserSettingsRequest>
{
    public PatchUserSettingsRequestValidator() { }
}
