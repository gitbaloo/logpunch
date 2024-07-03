using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Login;
using Shared;

namespace WebPunchlog.Controllers;
[Authorize]
[ApiController]
[Route("api/consultant-overview")]
public class ConsultantOverviewController(IOverviewService overviewService) : ControllerBase
{
    [HttpGet("get-overview")]
    public async Task<IActionResult> GetOverview(bool sortAsc, bool showDaysWithNoRecords,
        bool setDefault, DateTime startDate, DateTime? endDate, string timePeriod, string timeMode, string groupBy,
        string thenBy)
    {
        try
        {
            var token = HttpContext.Request.Headers.Authorization.ToString().Replace("Bearer ", "");
            var response = await overviewService.OverviewQuery(token, sortAsc, showDaysWithNoRecords, setDefault,
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
            var response = await overviewService.GetDefaultQuery(token);
            return Ok(response);
        }
        catch (ArgumentException e)
        {
            return BadRequest(e.Message);
        }
    }
}
