using Microsoft.AspNetCore.Mvc;
using ScreenTimeTracker.Application.Interfaces;

namespace ScreenTimeTracker.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsageReportsController(IUsageReportQueries usageReportQueries) : ControllerBase
{
    private readonly IUsageReportQueries _usageReportQueries = usageReportQueries;
}
