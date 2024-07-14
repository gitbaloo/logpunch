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

    [HttpGet("get-ongoing")]
    public async Task<IActionResult> GetOngoingRegistration(Guid? employeeId)
    {
        try
        {
            var token = HttpContext.Request.Headers.Authorization.ToString().Replace("Bearer ", "");
            var user = await _loginService.ValidateToken(token);
            Guid nonNullableEmployeeId;

            if (!employeeId.HasValue)
            {
                nonNullableEmployeeId = user.Id;
            }
            else
            {
                nonNullableEmployeeId = employeeId.Value;
            }

            var response = await _overviewService.GetOngoingRegistration(user.Id, nonNullableEmployeeId);

            if (response is null)
            {
                return NotFound(new { Message = "No ongoing registrations could be found." });
            }

            return Ok(response);
        }
        catch (ArgumentException e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpGet("get-unsettled-work")]
    public async Task<IActionResult> GetUnsettledWorkRegistrations(Guid? employeeId)
    {
        try
        {
            var token = HttpContext.Request.Headers.Authorization.ToString().Replace("Bearer ", "");
            var user = await _loginService.ValidateToken(token);
            Guid nonNullableEmployeeId;

            if (!employeeId.HasValue)
            {
                nonNullableEmployeeId = user.Id;
            }
            else
            {
                nonNullableEmployeeId = employeeId.Value;
            }

            var response = await _overviewService.GetUnsettledWorkRegistrations(user.Id, nonNullableEmployeeId);

            if (response.Count == 0)
            {
                return NotFound(new { Message = "No unsettled work registrations could be found." });
            }

            return Ok(response);
        }
        catch (ArgumentException e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpGet("get-unsettled-absence")]
    public async Task<IActionResult> GetUnsettledAbsenceRegistrations(Guid? employeeId)
    {
        try
        {
            var token = HttpContext.Request.Headers.Authorization.ToString().Replace("Bearer ", "");
            var user = await _loginService.ValidateToken(token);
            Guid nonNullableEmployeeId;

            if (!employeeId.HasValue)
            {
                nonNullableEmployeeId = user.Id;
            }
            else
            {
                nonNullableEmployeeId = employeeId.Value;
            }

            var response = await _overviewService.GetUnsettledAbsenceRegistrations(user.Id, nonNullableEmployeeId);

            if (response.Count == 0)
            {
                return NotFound(new { Message = $"No unsettled absence registrations could be found." });
            }

            return Ok(response);
        }
        catch (ArgumentException e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpGet("get-unsettled-transportation")]
    public async Task<IActionResult> GetUnsettledTransportationRegistrations(Guid? employeeId)
    {
        try
        {
            var token = HttpContext.Request.Headers.Authorization.ToString().Replace("Bearer ", "");
            var user = await _loginService.ValidateToken(token);
            Guid nonNullableEmployeeId;

            if (!employeeId.HasValue)
            {
                nonNullableEmployeeId = user.Id;
            }
            else
            {
                nonNullableEmployeeId = employeeId.Value;
            }

            var response = await _overviewService.GetUnsettledTransportationRegistrations(user.Id, nonNullableEmployeeId);

            if (response.Count == 0)
            {
                return NotFound(new { Message = $"No unsettled transportation registrations could be found." });
            }

            return Ok(response);
        }
        catch (ArgumentException e)
        {
            return BadRequest(e.Message);
        }
    }


    [HttpGet("get-overview")]
    public async Task<IActionResult> GetWorkOverview(bool sortAsc, bool showDaysWithNoRecords,
        bool setDefault, DateTime startDate, DateTime? endDate, string timePeriod, string timeMode, string groupBy,
        string thenBy)
    {
        try
        {
            var token = HttpContext.Request.Headers.Authorization.ToString().Replace("Bearer ", "");
            var user = await _loginService.ValidateToken(token);
            var response = await _overviewService.WorkOverviewQuery(user.Id, sortAsc, showDaysWithNoRecords, setDefault,
                startDate, endDate, timePeriod, timeMode, groupBy, thenBy);
            return Ok(response);
        }
        catch (ArgumentException e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpGet("get-default-overview")]
    public async Task<IActionResult> GetDefaultOverview()
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
