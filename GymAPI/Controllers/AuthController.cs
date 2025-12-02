using GymAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace GymAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        [HttpPost("login")]
        public IActionResult Login([FromBody] Login login)
        {
            var validUsername = Environment.GetEnvironmentVariable("LOGIN_USERNAME");
            var validPassword = Environment.GetEnvironmentVariable("LOGIN_PASSWORD");

            if (string.IsNullOrEmpty(validUsername) || string.IsNullOrEmpty(validPassword))
                return StatusCode(500, "LOGIN_USERNAME or LOGIN_PASSWORD is not set");

            if (login.Username != validUsername || login.Password != validPassword)
                return Unauthorized("Invalid credentials");
            return Ok();
        }
    }
}
