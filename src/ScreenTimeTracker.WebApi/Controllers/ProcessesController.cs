using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using ScreenTimeTracker.Application.DTOs;
using ScreenTimeTracker.Application.Exceptions;
using ScreenTimeTracker.Application.Interfaces;
using ScreenTimeTracker.Domain.Interfaces;
using ScreenTimeTracker.WebApi.DTOs.Processes;

namespace ScreenTimeTracker.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProcessesController(IProcessInfoRepository processInfoRepository, IProcessQueries processQueries) : ControllerBase
{
    private readonly IProcessInfoRepository _processInfoRepository = processInfoRepository;
    private readonly IProcessQueries _processQueries = processQueries;

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProcessInfoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllProcessesAsync()
    {
        var processes = await _processQueries.GetAllProcessesAsync();
        return Ok(processes);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProcessInfoDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProcessByIdAsync(Guid id)
    {
        ProcessInfoDto? processDto = await _processQueries.GetProcessByIdAsync(id);
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

    [HttpPut("{id:guid}/alias")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProcessAliasAsync(Guid id, [FromBody] UpdateAliasRequest updateDto)
    {
        var processInfo = await _processInfoRepository.GetByIdAsync(id);
        if (processInfo is null)
            return NotFound();
        Console.WriteLine(updateDto.Alias);
        processInfo.UpdateUserDetails(updateDto.Alias, processInfo.AutoUpdate, processInfo.IconPath);
        Console.WriteLine(processInfo.Alias);
        await _processInfoRepository.UpdateAsync(processInfo);
        return NoContent();
    }

    [HttpPut("{id:guid}/auto-update")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProcessAutoUpdateAsync(Guid id, [FromBody] UpdateAutoUpdateRequest updateDto)
    {
        var processInfo = await _processInfoRepository.GetByIdAsync(id);
        if (processInfo is null)
            return NotFound();
        processInfo.UpdateUserDetails(processInfo.Alias, updateDto.AutoUpdate, processInfo.IconPath);
        await _processInfoRepository.UpdateAsync(processInfo);
        return NoContent();
    }

    [HttpPut("{id:guid}/icon-path")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProcessIconPathAsync(Guid id, [FromBody] UpdateIconPathRequest updateDto)
    {
        var processInfo = await _processInfoRepository.GetByIdAsync(id);
        if (processInfo is null)
            return NotFound();
        processInfo.UpdateUserDetails(processInfo.Alias, processInfo.AutoUpdate, updateDto.IconPath);
        await _processInfoRepository.UpdateAsync(processInfo);
        return NoContent();
    }

    [HttpGet("{id:guid}/icon")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> GetProcessIconByIdAsync(Guid id)
    {
        string? iconPath;
        try
        {
            iconPath = await _processQueries.GetProcessIconByIdAsync(id);
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
}