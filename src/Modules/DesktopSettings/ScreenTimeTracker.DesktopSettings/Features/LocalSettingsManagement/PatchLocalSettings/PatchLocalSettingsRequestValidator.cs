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
        When(
            x => x.DefaultUIOpenMode.HasValue,
            () =>
            {
                RuleFor(x => x.DefaultUIOpenMode)
                    .IsInEnum()
                    .WithMessage("无效的界面打开模式，可选值为 Window 或 Tray");
            }
        );
    }
}
