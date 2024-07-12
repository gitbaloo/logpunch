using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Login;

namespace Logpunch.Controllers
{
    [ApiController]
    [Route("api/profile")]
    public class ProfileController : ControllerBase
    {
        private readonly ILoginService _loginService;

        public ProfileController(ILoginService loginService)
        {
            _loginService = loginService;
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult> GetProfile()
        {
            try
            {
                var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                var user = await _loginService.ValidateToken(token);
                return Ok(user);
            }
            catch (Exception e)
            {
                return ValidationProblem(e.Message);
            }
        }
    }
}
