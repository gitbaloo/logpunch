using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Login;
using Shared;

namespace Logpunch.Controllers;
[Authorize]
[ApiController]
[Route("api/overview")]
public class OverviewController : ControllerBase
{
    private readonly IOverviewService _overviewService;
    private readonly ILoginService _loginService;

    public OverviewController(IOverviewService overviewService, ILoginService loginService)
    {
        _overviewService = overviewService;
        _loginService = loginService;
    }

    [HttpGet("get-overview")]
    public async Task<IActionResult> GetOverview(bool sortAsc, bool showDaysWithNoRecords,
        bool setDefault, DateTime startDate, DateTime? endDate, string timePeriod, string timeMode, string groupBy,
        string thenBy, string registrationTypeString)
    {
        try
        {
            var token = HttpContext.Request.Headers.Authorization.ToString().Replace("Bearer ", "");
            var user = await _loginService.ValidateToken(token);
            var response = await _overviewService.OverviewQuery(user.Id, sortAsc, showDaysWithNoRecords, setDefault,
                startDate, endDate, timePeriod, timeMode, groupBy, thenBy, registrationTypeString);
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
            var user = await _loginService.ValidateToken(token);
            var response = await _overviewService.GetDefaultQuery(user.Id);
            return Ok(response);
        }
        catch (ArgumentException e)
        {
            return BadRequest(e.Message);
        }
    }
}
