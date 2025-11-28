using Mediator;
using ScreenTimeTracker.ApplicationLayer.Common.DTOs;

namespace ScreenTimeTracker.ApplicationLayer.Features.Configuration.Queries.GetAggregationSettings;

public record GetAggregationSettingsQuery : IQuery<AggregationSettingsDto>
{
}