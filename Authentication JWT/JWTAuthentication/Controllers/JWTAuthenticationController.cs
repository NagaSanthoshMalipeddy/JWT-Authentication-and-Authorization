using JWTAuthentication.Entities;
using JWTAuthentication.Models;
using JWTAuthentication.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JWTAuthentication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JWTAuthenticationController(IAuthService authService) : ControllerBase
    {   
        [HttpPost]
        [Route("register")]
        public IActionResult Register([FromBody] UserDTO user)
        {
            if (!authService.Register(user))
            {
                return BadRequest($"{user.UserName} already exist");
            }
            return Created();
        }

        [HttpPost]
        [Route("login")]
        public IActionResult LogIn([FromBody] UserDTO user)
        {
            TokenDTO? token = authService.LogIn(user);
            if (token == null)
            {
                return BadRequest(user);
            }
            return Ok(token);
        }

        [HttpPost]
        [Route("newTokens")]
        [Authorize]
        public IActionResult GetNewTokens([FromBody] UserWithToken user)
        {
            TokenDTO? token = authService.GenerateNewToken(user);
            if (token == null)
            {
                return BadRequest(user);
            }
            return Ok(token);
        }

        [HttpGet]
        [Route("privateAPI")]
        [Authorize]
        public IActionResult SecureCall()
        {
            return Ok("Your are authenticated and connection is secure");
        }

        [HttpGet]
        [Route("privateAPI/admins-only")]
        [Authorize(Roles = "Admin")]
        public IActionResult AdminsAPI()
        {
            return Ok("You are Administrator");
        }
    }
}
