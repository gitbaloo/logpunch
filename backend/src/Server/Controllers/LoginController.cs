using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Login;

namespace WebPunchlog.Controllers;

[ApiController]
[Route("api/login")]
public class LoginController(ILoginService loginService) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("authorize")]
    public async Task<IActionResult> AuthorizeLogin([FromForm] string email, [FromForm] string password)
    {
        try
        {
            var user = await loginService.AuthorizeLogin(email, password);
            return Ok(user);
        }
        catch (ArgumentException e)
        {
            return Unauthorized(e.Message);
        }
    }

    [HttpGet("authenticate")]
    public async Task<ActionResult> AuthenticateUser(string token)
    {
        try
        {
            var user = await loginService.ValidateToken(token);
            return Ok(user);
        }
        catch (Exception e)
        {
            ValidationProblem(e.Message);
        }
        return ValidationProblem("Unable to validate user");
    }
}
