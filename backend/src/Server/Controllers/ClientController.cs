// using Domain;
// using Infrastructure.Customer;
// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Mvc;
// using Shared;
// using System.Collections.Generic;
// using System.Threading.Tasks;

// namespace WebPunchlog.Controllers;

// // [Authorize]
// [ApiController]
// [Route("api/customers")]
// public class ClientController : ControllerBase
// {
//     private readonly ICustomerService _clientService;

//     public ClientController(ICustomerService customerService)
//     {
//         _clientService = customerService;
//     }

//     [HttpGet("{employeeId}")]
//     public async Task<ActionResult<List<LogpunchClientDto>>> GetClients(int employeeId)
//     {
//         var clients = await _clientService.GetCustomers(employeeId);
//         return Ok(clients);
//     }
// }
