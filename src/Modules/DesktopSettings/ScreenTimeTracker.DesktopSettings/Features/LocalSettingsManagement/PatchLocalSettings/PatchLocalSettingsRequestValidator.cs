using FastEndpoints;
using FluentValidation;
using ScreenTimeTracker.DesktopSettings.Domain;

namespace ScreenTimeTracker.DesktopSettings.Features.LocalSettingsManagement.PatchLocalSettings;

public class UpdateLocalSettingsRequestValidator : Validator<PatchLocalSettingsRequest>
{
    public UpdateLocalSettingsRequestValidator()
    {
        When(
            x => x.Language.HasValue,
            () =>
            {
                RuleFor(x => x.Language.Value)
                    .Must(lang => LocalSettings.SupportedLanguages.Contains(lang))
                    .WithMessage(
                        "Unsupported language type. Supported languages: "
                            + string.Join(", ", LocalSettings.SupportedLanguages)
                    );
            }
        );
    }
}
