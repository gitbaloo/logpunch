using Domain;
using Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Login;
using Shared;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Logpunch.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/clients")]
    public class ClientController : ControllerBase
    {
        private readonly IClientService _clientService;
        private readonly ILoginService _loginService;

        public ClientController(IClientService clientService, ILoginService loginService)
        {
            _clientService = clientService;
            _loginService = loginService;
        }

        [HttpGet("get-all")]
        public async Task<ActionResult<List<LogpunchClientDto>>> GetClients(Guid? employeeId)
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

            var clients = await _clientService.GetClients(nonNullableEmployeeId);

            if (clients.Count == 0)
            {
                return NotFound(new { Message = "No clients could be found for this employee" });
            }

            return Ok(clients);
        }
    }
}
