using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Login;
using Shared;
using Infrastructure;

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

    // Work registrations

    [HttpGet("work/get-ongoing")]
    public async Task<IActionResult> GetOngoingRegistration(Guid? employeeId)
    {
        try
        {
            var token = HttpContext.Request.Headers.Authorization.ToString().Replace("Bearer ", "");
            var user = await _loginService.ValidateToken(token);

            var response = await _overviewService.GetOngoingRegistration(user.Id, employeeId);

            return Ok(response);
        }
        catch (ArgumentException e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpGet("work/get-unsettled")]
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

    [HttpGet("work/get-overview")]
    public async Task<IActionResult> GetWorkOverview(Guid? employeeId, bool sortAsc, bool showDaysWithNoRecords,
        bool setDefault, DateTime startDate, DateTime? endDate, string timePeriod, string timeMode, string groupBy,
        string thenBy)
    {
        try
        {
            var token = HttpContext.Request.Headers.Authorization.ToString().Replace("Bearer ", "");
            var user = await _loginService.ValidateToken(token);
            var response = await _overviewService.WorkOverviewQuery(user.Id, employeeId, sortAsc, showDaysWithNoRecords, setDefault,
                startDate, endDate, timePeriod, timeMode, groupBy, thenBy);
            return Ok(response);
        }
        catch (ArgumentException e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpGet("work/get-default-overview")]
    public async Task<IActionResult> GetDefaultWorkOverview()
    {
        try
        {
            var token = HttpContext.Request.Headers.Authorization.ToString().Replace("Bearer ", "");
            var user = await _loginService.ValidateToken(token);
            var response = await _overviewService.GetDefaultWorkQuery(user.Id);
            return Ok(response);
        }
        catch (ArgumentException e)
        {
            return BadRequest(e.Message);
        }
    }

    // Transportation registrations

    [HttpGet("transportation/get-unsettled")]
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

    [HttpGet("transportation/get-overview")]
    public async Task<IActionResult> GetTransportationOverview(Guid? employeeId, bool sortAsc, bool showDaysWithNoRecords, DateTime startDate, DateTime? endDate, string timePeriod, string timeMode, string groupBy,
        string thenBy)
    {
        try
        {
            var token = HttpContext.Request.Headers.Authorization.ToString().Replace("Bearer ", "");
            var user = await _loginService.ValidateToken(token);
            var response = await _overviewService.TransportationOverviewQuery(user.Id, employeeId, sortAsc, showDaysWithNoRecords,
                startDate, endDate, timePeriod, timeMode, groupBy, thenBy);
            return Ok(response);
        }
        catch (ArgumentException e)
        {
            return BadRequest(e.Message);
        }
    }

    // Absence registrations

    [HttpGet("absence/get-unsettled")]
    public async Task<IActionResult> GetUnsettledAbsenceRegistrations(Guid? employeeId) // employeeId will be null unless an admin using function to check an employees registrations
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

    [HttpGet("absence/get-overview")]
    public async Task<IActionResult> GetAbsenceOverview(Guid? employeeId, bool sortAsc, bool showDaysWithNoRecords,
        DateTime startDate, DateTime? endDate, string timePeriod, string timeMode, string groupBy, string thenBy, string absenceType)
    {
        try
        {
            var token = HttpContext.Request.Headers.Authorization.ToString().Replace("Bearer ", "");
            var user = await _loginService.ValidateToken(token);
            var response = await _overviewService.AbsenceOverviewQuery(user.Id, employeeId, sortAsc, showDaysWithNoRecords,
                startDate, endDate, timePeriod, timeMode, groupBy, thenBy, absenceType);
            return Ok(response);
        }
        catch (ArgumentException e)
        {
            return BadRequest(e.Message);
        }
    }
}
