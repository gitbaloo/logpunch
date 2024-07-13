using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Login;
using Shared;

namespace Logpunch.Controllers;
[Authorize]
[ApiController]
[Route("api/employee-overview")]
public class OverviewController : ControllerBase
{
    private readonly IOverviewService _overviewService;

    public OverviewController(IOverviewService overviewService)
    {
        _overviewService = overviewService;
    }

    [HttpGet("get-overview")]
    public async Task<IActionResult> GetWorkOverview(bool sortAsc, bool showDaysWithNoRecords,
        bool setDefault, DateTime startDate, DateTime? endDate, string timePeriod, string timeMode, string groupBy,
        string thenBy)
    {
        try
        {
            var token = HttpContext.Request.Headers.Authorization.ToString().Replace("Bearer ", "");
            var response = await _overviewService.OverviewQuery(token, sortAsc, showDaysWithNoRecords, setDefault,
                startDate,
                endDate, timePeriod,
                timeMode, groupBy, thenBy);
            return Ok(response);
        }
        catch (ArgumentException e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpGet("get-default-query")]
    public async Task<IActionResult> GetDefaultQuery()
    {
        try
        {
            var token = HttpContext.Request.Headers.Authorization.ToString().Replace("Bearer ", "");
            var response = await _overviewService.GetDefaultQuery(token);
            return Ok(response);
        }
        catch (ArgumentException e)
        {
            return BadRequest(e.Message);
        }
    }
}
