using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ScreenTimeTracker.ApplicationLayer.Common.DTOs;
using ScreenTimeTracker.ApplicationLayer.Features.Configuration.Commands.ResetAggregationSettings;
using ScreenTimeTracker.ApplicationLayer.Features.Configuration.Commands.ResetTrackerSettings;
using ScreenTimeTracker.ApplicationLayer.Features.Configuration.Commands.UpdateAggregationSettings;
using ScreenTimeTracker.ApplicationLayer.Features.Configuration.Commands.UpdateTrackerSettings;
using ScreenTimeTracker.ApplicationLayer.Features.Configuration.Queries.GetAggregationSettings;
using ScreenTimeTracker.ApplicationLayer.Features.Configuration.Queries.GetTrackerSettings;
using ScreenTimeTracker.Desktop.WebApi.DTOs;
using ScreenTimeTracker.Desktop.WebApi.Mappers;

namespace ScreenTimeTracker.Desktop.WebApi.Controllers;

[ApiController]
[Route("api/configuration")]
public class ConfigurationController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    [HttpGet("tracker")]
    [ProducesResponseType(typeof(TrackerSettingsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTrackerSettingsAsync()
    {
        TrackerSettingsDto trackerSettings = await _mediator.Send(new GetTrackerSettingsQuery());
        return Ok(TrackerSettingsMapper.Map(trackerSettings));
    }

    [HttpPut("tracker")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateTrackerSettingsAsync(UpdateTrackerSettingsRequest request)
    {
        await _mediator.Send(
            new UpdateTrackerSettingsCommand(
                TrackerSettingsMapper.Map(request)));
        return NoContent();
    }

    [HttpPost("tracker/reset")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ResetTrackerSettingsAsync()
    {
        await _mediator.Send(new ResetTrackerSettingsCommand());
        return NoContent();
    }

    [HttpGet("aggregation")]
    [ProducesResponseType(typeof(AggregationSettingsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAggregationSettingsAsync()
    {
        AggregationSettingsDto aggregationSettings = await _mediator.Send(new GetAggregationSettingsQuery());
        return Ok(AggregationSettingsMapper.Map(aggregationSettings));
    }

    [HttpPut("aggregation")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateAggregationSettingsAsync(UpdateAggregationSettingsRequest request)
    {
        await _mediator.Send(
            new UpdateAggregationSettingsCommand(
                AggregationSettingsMapper.Map(request)));
        return NoContent();
    }

    [HttpPost("aggregation/reset")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ResetAggregationSettingsAsync()
    {
        await _mediator.Send(new ResetAggregationSettingsCommand());
        return NoContent();
    }
}
