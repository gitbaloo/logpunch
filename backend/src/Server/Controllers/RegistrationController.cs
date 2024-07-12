using Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Login;
using System.Security.Claims;
using Shared;

namespace Logpunch.Controllers;

[Authorize]
[ApiController]
[Route("api/registration")]
public class RegistrationController : ControllerBase
{
    private readonly IRegistrationService _registrationService;
    private readonly ILoginService _loginService;

    public RegistrationController(IRegistrationService registrationService, ILoginService loginService)
    {
        _registrationService = registrationService;
        _loginService = loginService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateRegistration([FromBody] CreateRegistrationRequest request)
    {
        try
        {
            var userId = await GetUserIdFromToken();
            var registration = await _registrationService.CreateRegistration(userId, request.EmployeeId, request.ClientId, request.Start, request.End, request.FirstComment, request.SecondComment);
            return Ok(registration);
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPost("shift/start")]
    public async Task<IActionResult> StartShiftRegistration([FromBody] StartShiftRegistrationRequest request)
    {
        try
        {
            var userId = await GetUserIdFromToken();
            var startShiftRegistration = await _registrationService.StartShiftRegistration(userId, request.EmployeeId, request.ClientId, request.FirstComment);
            return Ok(startShiftRegistration);
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPatch("shift/end")]
    public async Task<IActionResult> EndShiftRegistration([FromBody] EndShiftRegistrationRequest request)
    {
        try
        {
            var userId = await GetUserIdFromToken();
            var endShiftRegistration = await _registrationService.EndShiftRegistration(userId, request.EmployeeId, request.RegistrationId, request.SecondComment);
            if (endShiftRegistration is null)
            {
                return NotFound();
            }
            return Ok(endShiftRegistration);
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPatch("admin/statusupdate")]
    public async Task<IActionResult> UpdateRegistrationStatus([FromBody] UpdateStatusRequest request)
    {
        try
        {
            var userId = await GetUserIdFromToken();

            var registration = await _registrationService.UpdateRegistrationStatus(userId, request.RegistrationId, request.Status);
            if (registration is null)
            {
                return NotFound();
            }
            return Ok(registration);
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPatch("admin/typechange")]
    public async Task<IActionResult> ChangeRegistrationType([FromBody] ChangeTypeRequest request)
    {
        try
        {
            var userId = await GetUserIdFromToken();

            var registration = await _registrationService.UpdateRegistrationStatus(userId, request.RegistrationId, request.Type);
            if (registration is null)
            {
                return NotFound();
            }
            return Ok(registration);
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(500, ex.Message);
        }
    }


    private async Task<Guid> GetUserIdFromToken()
    {
        var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        var user = await _loginService.ValidateToken(token);
        return user.Id;
    }
}
