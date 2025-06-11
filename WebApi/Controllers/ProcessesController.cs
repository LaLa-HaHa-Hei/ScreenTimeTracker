using System.ComponentModel.DataAnnotations;
using Data;
using Microsoft.AspNetCore.Mvc;
using WebApi.DTOs.Processes;
using WebApi.Options;



namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProcessesController(ScreenTimeContext context, AppOptions options, ILogger<ProcessesController> logger) : ControllerBase
    {
        private readonly ScreenTimeContext _context = context;
        private readonly AppOptions _options = options;
        private readonly ILogger<ProcessesController> _logger = logger;

        [HttpGet("name/{name}")]
        public async Task<ProcessInfoResponse?> GetProcessByName(string name)
        {
            var processInfo = await _context.ProcessInfos.FindAsync(name);
            if (processInfo == null)
            {
                _logger.LogWarning("Process with name **{ProcessName}** not found.", name);
                return null;
            }

            return new ProcessInfoResponse
            {
                ProcessName = processInfo.ProcessName,
                Alias = processInfo.Alias,
                ExecutablePath = processInfo.ExecutablePath,
                // 替换本地路径为 Web 请求路径
                IconPath = processInfo.IconPath?.Replace(_options.DataDirPath, _options.DataRequestPath),
                Description = processInfo.Description
            };
        }

        [HttpPut("name/{name}/alias")]
        public async Task<IActionResult> UpdateProcessAlias([Required] string name, [FromBody] UpdateAliasRequest request)
        {
            var processInfo = await _context.ProcessInfos.FindAsync(name);
            if (processInfo == null)
            {
                _logger.LogWarning("Process with name **{ProcessName}** not found.", name);
                return NotFound(new { Message = "Process not found." });
            }
            processInfo.Alias = request.Alias;
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Alias updated successfully" });
        }
    }
}
