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
    public async Task<IActionResult> CreateRegistration([FromBody] CreateEmployeeRegistrationRequest request)
    {
        try
        {
            var timeRegistered = await RegistrationService.CreateRegistration(request.EmployeeId, request.ClientId, request.Hours);
            return Ok(timeRegistered);
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateRegistrationStatus(int id, [FromBody] RegistrationUpdateRequest request)
    {
        try
        {
            var updatedRegistration = await _registrationService.UpdateRegistration(id, request);
            if (updatedRegistration == null)
            {
                return NotFound();
            }
            return Ok(updatedRegistration);
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> EndOpenRegistration(int id, [FromBody] RegistrationUpdateRequest request)
    {
        try
        {
            var updatedRegistration = await _registrationService.UpdateRegistration(id, request);
            if (updatedRegistration == null)
            {
                return NotFound();
            }
            return Ok(updatedRegistration);
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

}
