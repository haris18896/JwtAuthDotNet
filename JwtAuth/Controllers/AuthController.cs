using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using JwtAuth.Entities;
using JwtAuth.Entities.Models;
using Microsoft.AspNetCore.Identity;

namespace JwtAuth.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        public static User user = new();
        [HttpPost("register")]
        public ActionResult<User> Register(UserDto request)
        {
            // Here you would typically add logic to save the user to a database
            var hashedPassword = new PasswordHasher<User>()
             .HashPassword(user, request.Password);

            user.Username = request.Username;
            user.PasswordHash = hashedPassword;

            return Ok(user);
        }

        [HttpPost("login")]
        public ActionResult<string> Login(UserDto request)
        {
            Console.WriteLine(request);
            // Here you would typically validate the user against a database
            if (user.Username != request.Username)
            {
                return BadRequest("User not Found");
            }

            if (string.IsNullOrEmpty(user.Username) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest("Username or Password cannot be empty");
            }

            if (new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash, request.Password) == PasswordVerificationResult.Failed)
            {
                return BadRequest("Invalid Password");
            }

            string token = "success";

            return Ok(token);
        }
    }
}
