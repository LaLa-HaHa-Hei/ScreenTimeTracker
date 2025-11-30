using System.IO;
using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using ScreenTimeTracker.ApplicationLayer.Common.DTOs;
using ScreenTimeTracker.ApplicationLayer.Common.Exceptions;
using ScreenTimeTracker.ApplicationLayer.Features.ProcessInfos.Queries.GetAllProcessInfos;
using ScreenTimeTracker.ApplicationLayer.Features.ProcessInfos.Queries.GetProcessIconPathById;
using ScreenTimeTracker.ApplicationLayer.Features.ProcessInfos.Queries.GetProcessInfoById;
using ScreenTimeTracker.ApplicationLayer.Features.UsageReport.Queries.GetProcessDailyDistributionForPeriod;
using ScreenTimeTracker.ApplicationLayer.Features.UsageReport.Queries.GetProcessHourlyDistributionForDay;
using ScreenTimeTracker.Desktop.WebApi.DTOs;
using ScreenTimeTracker.DomainLayer.Interfaces;

namespace ScreenTimeTracker.Desktop.WebApi.Controllers;

[ApiController]
[Route("api/processes")]
public class ProcessesController(IProcessInfoRepository processInfoRepository, IMediator mediator) : ControllerBase
{
    private readonly IProcessInfoRepository _processInfoRepository = processInfoRepository;
    private readonly IMediator _mediator = mediator;

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProcessInfoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllProcessesAsync()
    {
        var processes = await _mediator.Send(new GetAllProcessInfosQuery());
        return Ok(processes);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProcessInfoDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProcessAsync(Guid id)
    {
        ProcessInfoDto? processDto = await _mediator.Send(new GetProcessInfoByIdQuery(id));
        if (processDto is null)
            return NotFound();

        return Ok(processDto);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProcessAsync(Guid id)
    {
        try
        {
            await _processInfoRepository.DeleteAsync(id);
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProcessAsync(Guid id, [FromBody] UpdateProcessRequest updateDto)
    {
        var processInfo = await _processInfoRepository.GetByIdAsync(id);
        if (processInfo is null)
            return NotFound();
        processInfo.UpdateUserDetails(updateDto.Alias, updateDto.AutoUpdate, updateDto.IconPath);
        await _processInfoRepository.UpdateAsync(processInfo);
        return NoContent();
    }

    [HttpGet("{id:guid}/icon")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> GetProcessIconAsync(Guid id)
    {
        string? iconPath;
        try
        {
            iconPath = await _mediator.Send(
                new GetProcessIconPathByIdQuery(id)
            );
        }
        catch (NotFoundException)
        {
            return NotFound();
        }

        if (iconPath is null)
            return NoContent();
        iconPath = Path.GetFullPath(iconPath);
        var provider = new FileExtensionContentTypeProvider();
        if (!provider.TryGetContentType(iconPath, out var contentType))
            contentType = "image/png";
        return PhysicalFile(iconPath, contentType);
    }

    [HttpGet("{id:guid}/usage-distribution/hourly")]
    [ProducesResponseType(typeof(IDictionary<int, long>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ProcessHourlyDistribution(Guid id, [FromQuery] DateOnly date)
    {
        return Ok(await _mediator.Send(
            new GetProcessHourlyDistributionForDayQuery(id, date)
        ));
    }

    [HttpGet("{id:guid}/usage-distribution/daily")]
    [ProducesResponseType(typeof(IDictionary<DateOnly, long>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ProcessDailyDistribution(Guid id, [FromQuery] DateOnly startDate, [FromQuery] DateOnly endDate)
    {
        return Ok(await _mediator.Send(
            new GetProcessDailyDistributionForPeriodQuery(id, startDate, endDate)
        ));
    }
}