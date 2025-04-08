using AppointmentManagementAPI.Models;
using AppointmentManagementAPI.Services;
using AppointmentManagementAPI.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using AppointmentManagementAPI.Services.Interfaces;

namespace AppointmentManagementAPI.Controllers
{
    //[ApiExplorerSettings(GroupName = "1. Authentication")]
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IJwtService _jwtService;

        public AuthController(IAuthService authService, IJwtService jwtService)
        {
            _authService = authService;
            _jwtService = jwtService;
        }

        [HttpPost("signup")]
        public async Task<IActionResult> Signup([FromBody] UserDto user)
        {
            var result = await _authService.RegisterUserAsync(user);
            if (!result)
            {
                return BadRequest(new { message = "User registration failed. Username or email might already exist." });
            }
            return Ok(new { message = "User registered successfully." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var token = await _authService.AuthenticateAsync(loginDto);
            if (token == null)
                return Unauthorized(new { Message = "Invalid username or password" });

            return Ok(new { Token = token });
        }

    }
}
