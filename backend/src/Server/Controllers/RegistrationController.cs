using Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared;

namespace WebPunchlog.Controllers;

// [Authorize]
[ApiController]
[Route("api/registration")]
public class RegistrationController(IRegistrationService RegistrationService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> EmployeeCreateRegistration(Guid userId, [FromBody] CreateEmployeeRegistrationRequest request)
    {
        try
        {
            var registration = await RegistrationService.EmployeeCreateRegistration(userId, request.EmployeeId, request.ClientId, request.Start, request.End, request.InternalComment);
            return Ok(registration);
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
    [HttpPost("shift/start")]
    public async Task<IActionResult> StartShiftRegistration(Guid userId, [FromBody] StartShiftRegistrationRequest request)
    {
        try
        {
            var startShiftRegistration = await RegistrationService.StartShiftRegistration(userId, request.EmployeeId, request.ClientId, request.InternalComment);
            return Ok(startShiftRegistration);
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPatch("shift/end")]
    public async Task<IActionResult> EndShiftRegistration(Guid userId, [FromBody] EndShiftRegistrationRequest request)
    {
        try
        {
            var endShiftRegistration = await RegistrationService.EndShiftRegistration(userId, request.EmployeeId, request.RegistrationId, request.InternalComment);
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

    // [HttpPatch("{id}")]
    // public async Task<IActionResult> AdminUpdateRegistrationStatus(int id, [FromBody] RegistrationUpdateRequest request)
    // {
    //     try
    //     {
    //         var updatedRegistration = await _registrationService.UpdateRegistration(id, request);
    //         if (updatedRegistration == null)
    //         {
    //             return NotFound();
    //         }
    //         return Ok(updatedRegistration);
    //     }
    //     catch (HttpRequestException ex)
    //     {
    //         return StatusCode(500, ex.Message);
    //     }
    // }
}
