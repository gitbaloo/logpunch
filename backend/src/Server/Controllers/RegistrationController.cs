using Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Login;
using System.Security.Claims;
using Shared;
using System.ComponentModel;

namespace Logpunch.Controllers
{
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

        // User APIs

        [HttpPost("work/create")]
        public async Task<IActionResult> CreateWorkRegistration([FromBody] CreateWorkRegistrationRequest request)
        {
            try
            {
                var token = HttpContext.Request.Headers.Authorization.ToString().Replace("Bearer ", "");
                var user = await _loginService.ValidateToken(token);
                Guid nonNullableEmployeeId;

                if (!request.EmployeeId.HasValue)
                {
                    nonNullableEmployeeId = user.Id;
                }
                else
                {
                    nonNullableEmployeeId = request.EmployeeId.Value;
                }

                var registration = await _registrationService.CreateWorkRegistration(user.Id, nonNullableEmployeeId, request.ClientId, request.Start, request.End, request.FirstComment, request.SecondComment);

                return Ok(registration);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("work/start")]
        public async Task<IActionResult> StartWorkRegistration([FromBody] StartRegistrationRequest request)
        {
            try
            {
                var token = HttpContext.Request.Headers.Authorization.ToString().Replace("Bearer ", "");
                var user = await _loginService.ValidateToken(token);
                Guid nonNullableEmployeeId;

                if (!request.EmployeeId.HasValue)
                {
                    nonNullableEmployeeId = user.Id;
                }
                else
                {
                    nonNullableEmployeeId = request.EmployeeId.Value;
                }

                var startRegistration = await _registrationService.StartWorkRegistration(user.Id, nonNullableEmployeeId, request.ClientId, request.FirstComment);

                return Ok(startRegistration);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPatch("work/end")]
        public async Task<IActionResult> EndWorkRegistration([FromBody] EndRegistrationRequest request)
        {
            try
            {
                var token = HttpContext.Request.Headers.Authorization.ToString().Replace("Bearer ", "");
                var user = await _loginService.ValidateToken(token);
                Guid nonNullableEmployeeId;

                if (!request.EmployeeId.HasValue)
                {
                    nonNullableEmployeeId = user.Id;
                }
                else
                {
                    nonNullableEmployeeId = request.EmployeeId.Value;
                }

                var endRegistration = await _registrationService.EndWorkRegistration(user.Id, nonNullableEmployeeId, request.RegistrationId, request.SecondComment);

                if (endRegistration is null)
                {
                    return NotFound("Ongoing registration was not found");
                }

                return Ok(endRegistration);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("transportation/create")]
        public async Task<IActionResult> CreateTransportationRegistration([FromBody] CreateWorkRegistrationRequest request)
        {
            try
            {
                var token = HttpContext.Request.Headers.Authorization.ToString().Replace("Bearer ", "");
                var user = await _loginService.ValidateToken(token);
                Guid nonNullableEmployeeId;

                if (!request.EmployeeId.HasValue)
                {
                    nonNullableEmployeeId = user.Id;
                }
                else
                {
                    nonNullableEmployeeId = request.EmployeeId.Value;
                }

                var registration = await _registrationService.CreateTransportationRegistration(user.Id, nonNullableEmployeeId, request.ClientId, request.Start, request.End, request.FirstComment, request.SecondComment);

                return Ok(registration);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("transportation/start")]
        public async Task<IActionResult> StartTransportationRegistration([FromBody] StartRegistrationRequest request)
        {
            try
            {
                var token = HttpContext.Request.Headers.Authorization.ToString().Replace("Bearer ", "");
                var user = await _loginService.ValidateToken(token);
                Guid nonNullableEmployeeId;

                if (!request.EmployeeId.HasValue)
                {
                    nonNullableEmployeeId = user.Id;
                }
                else
                {
                    nonNullableEmployeeId = request.EmployeeId.Value;
                }

                var startShiftRegistration = await _registrationService.StartTransportationRegistration(user.Id, nonNullableEmployeeId, request.ClientId, request.FirstComment);

                return Ok(startShiftRegistration);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPatch("transportation/end")]
        public async Task<IActionResult> EndTransportationRegistration([FromBody] EndRegistrationRequest request)
        {
            try
            {
                var token = HttpContext.Request.Headers.Authorization.ToString().Replace("Bearer ", "");
                var user = await _loginService.ValidateToken(token);
                Guid nonNullableEmployeeId;

                if (!request.EmployeeId.HasValue)
                {
                    nonNullableEmployeeId = user.Id;
                }
                else
                {
                    nonNullableEmployeeId = request.EmployeeId.Value;
                }

                var endRegistration = await _registrationService.EndTransportationRegistration(user.Id, nonNullableEmployeeId, request.RegistrationId, request.SecondComment);

                if (endRegistration is null)
                {
                    return NotFound("Ongoing registration was not found");
                }

                return Ok(endRegistration);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


        [HttpPatch("confirmation")]
        public async Task<IActionResult> EmployeeConfirmationRegistration([FromBody] EmployeeConfirmationRegistrationRequest request)
        {
            try
            {
                var token = HttpContext.Request.Headers.Authorization.ToString().Replace("Bearer ", "");
                var user = await _loginService.ValidateToken(token);

                var confirmedRegistration = await _registrationService.EmployeeConfirmationRegistration(user.Id, request.RegistrationId);

                if (confirmedRegistration is null)
                {
                    return NotFound("Registration was not found");
                }

                return Ok(confirmedRegistration);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("correction")]
        public async Task<IActionResult> EmployeeCorrectionRegistration([FromBody] EmployeeCorrectionRegistrationRequest request)
        {
            try
            {
                var token = HttpContext.Request.Headers.Authorization.ToString().Replace("Bearer ", "");
                var user = await _loginService.ValidateToken(token);

                var correctionRegistration = await _registrationService.EmployeeCorrectionRegistration(user.Id, request.Start, request.End, request.ClientId, request.FirstComment, request.SecondComment, request.CorrectionOfId);

                if (correctionRegistration is null)
                {
                    return NotFound("Registration was not found");
                }

                return Ok(correctionRegistration);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // Admin APIs

        [HttpPost("admin/create/absence")]
        public async Task<IActionResult> CreateAbsenceRegistration([FromBody] CreateAbsenceRegistrationRequest request)
        {
            try
            {
                var token = HttpContext.Request.Headers.Authorization.ToString().Replace("Bearer ", "");
                var user = await _loginService.ValidateToken(token);
                var registration = await _registrationService.CreateAbsenceRegistration(user.Id, request.EmployeeId, request.Start, request.End, request.Type, request.FirstComment, request.SecondComment);

                if (registration is null)
                {
                    return NotFound("Registration was not found");
                }

                return Ok(registration);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPatch("admin/update")]
        public async Task<IActionResult> UpdateRegistrationStatus([FromBody] UpdateStatusRequest request)
        {
            try
            {
                var token = HttpContext.Request.Headers.Authorization.ToString().Replace("Bearer ", "");
                var user = await _loginService.ValidateToken(token);

                var registration = await _registrationService.UpdateRegistrationStatus(user.Id, request.RegistrationId, request.Status);

                if (registration is null)
                {
                    return NotFound("Registration was not found");
                }

                return Ok(registration);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPatch("admin/change")]
        public async Task<IActionResult> ChangeRegistrationType([FromBody] ChangeTypeRequest request)
        {
            try
            {
                var token = HttpContext.Request.Headers.Authorization.ToString().Replace("Bearer ", "");
                var user = await _loginService.ValidateToken(token);

                var registration = await _registrationService.UpdateRegistrationStatus(user.Id, request.RegistrationId, request.Type);

                if (registration is null)
                {
                    return NotFound("Registration was not found");
                }

                return Ok(registration);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


        [HttpPost("admin/correction")]
        public async Task<IActionResult> AdminCorrectionRegistration([FromBody] AdminCorrectionRegistrationRequest request)
        {
            try
            {
                var token = HttpContext.Request.Headers.Authorization.ToString().Replace("Bearer ", "");
                var user = await _loginService.ValidateToken(token);
                var correctionRegistration = await _registrationService.AdminCorrectionRegistration(user.Id, request.EmployeeId, request.Start, request.End, request.ClientId, request.FirstComment, request.SecondComment, request.CorrectionOfId);

                if (correctionRegistration is null)
                {
                    return NotFound("Registration was not found");
                }

                return Ok(correctionRegistration);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
