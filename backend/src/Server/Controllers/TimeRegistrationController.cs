using Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared;

namespace WebPunchlog.Controllers;

// [Authorize]
[ApiController]
[Route("api/time-registration")]
public class TimeRegistrationController(ITimeRegistrationService timeRegistrationService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> RegisterTime([FromBody] TimeRegistrationRequest request)
    {
        try
        {
            var timeRegistered = await timeRegistrationService.RegisterTime(request.ConsultantId, request.CustomerId, request.Hours);
            return Ok(timeRegistered);
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

}
