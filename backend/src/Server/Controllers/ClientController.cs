using Domain;
using Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        public ClientController(IClientService clientService)
        {
            _clientService = clientService;
        }

        [HttpGet("{employeeId}")]
        public async Task<ActionResult<List<LogpunchClientDto>>> GetClients(Guid employeeId)
        {
            var clients = await _clientService.GetClients(employeeId);
            return Ok(clients);
        }
    }
}
