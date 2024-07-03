using Domain;
using Infrastructure.Customer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebPunchlog.Controllers;

// [Authorize]
[ApiController]
[Route("api/customers")]
public class CustomerController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomerController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    [HttpGet("{consultantId}")]
    public async Task<ActionResult<List<CustomerDto>>> GetCustomers(int consultantId)
    {
        var customers = await _customerService.GetCustomers(consultantId);
        return Ok(customers);
    }
}
