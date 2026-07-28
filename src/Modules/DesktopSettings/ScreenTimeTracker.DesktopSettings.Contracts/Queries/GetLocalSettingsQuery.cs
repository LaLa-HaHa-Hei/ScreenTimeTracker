using Mediator;

namespace ScreenTimeTracker.DesktopSettings.Contracts.Queries;

public record GetLocalSettingsQuery() : IRequest<GetLocalSettingsResult>;
