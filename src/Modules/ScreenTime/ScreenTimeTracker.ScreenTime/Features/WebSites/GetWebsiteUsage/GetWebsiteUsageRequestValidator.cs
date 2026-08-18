using FastEndpoints;
using FluentValidation;

namespace ScreenTimeTracker.ScreenTime.Features.Websites.GetWebsiteUsage;

public class GetWebsiteUsageRequestValidator : Validator<GetWebsiteUsageRequest>
{
    public GetWebsiteUsageRequestValidator()
    {
        RuleFor(x => x.TimeZoneId)
            .Must(BeAValidTimeZone)
            .WithMessage(x =>
                $"The Provided TimeZoneId '{x.TimeZoneId}' is invalid or not supported by the system."
            );

        RuleFor(x => x.StartDate)
            .LessThanOrEqualTo(x => x.EndDate)
            .WithMessage(x =>
                $"The StartDate '{x.StartDate}' must be before or equal to the EndDate '{x.EndDate}'."
            );
    }

    private static bool BeAValidTimeZone(string timeZoneId)
    {
        try
        {
            TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
