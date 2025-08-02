# JwtAuthDotNet

* We are going to use JasonWeb token for authetication in ASP.net
* The features included are given below

1. Run your Project, alwasy reload after chagnes
2. add controller support to `program.cs` and map the controllers
3. `http://localhost:5008/openapi/v1.json` for checking json apis
4. `http://localhost:5008/scalar/v1` fro checking swagger

```sh
# Start your project
dotnet watch run 
 # or 
dotnet run

openssl rand -base64 64 # to generate base 64 secret to use in the appsettings.json for generation of jwt tokens

```

```cs
// .........
// 🔧 Add controller support
builder.Services.AddControllers();

// 🔧 Add OpenAPI support
builder.Services.AddOpenApi();  // you can add version param to this as well e.g // 🔧 builder.Services.AddOpenApi("v2");
// ........
// ........
// ........
// ........

// 🔧 Register controller endpoints
app.MapControllers();

app.Run();
```



### All Packages Required
* we aren't working in the VS, thats why we need to download the packages using terminal
* to check all the pacakges go to this link [NuGet Packages](https://www.nuget.org/PACKAGES)

```sh
dotnet add package Scalar.AspNetCore --version 2.6.6 # easy way to render beautiful API References based on OpenAPI/Swagger documents
dotnet add package System.IdentityModel.Tokens.Jwt --version 8.13.0 # this library simplifies working with OpenID Connect (OIDC), OAuth2.0, and JSON Web Tokens (JWT) in .NET.
dotnet add package Microsoft.EntityFrameworkCore # Entity Framework Core (EF Core) is a modern object-database mapper that lets you build a clean, portable, and high-level data access layer with .NET (C#) across a variety of databases

```

### Endpoints

1. Registering Users

* First of all we need 'Entity' to store our user in the data base for that we have created a directory `Entity`
* secondly we need `DTO (data transfer object)` to store the user
* thirdly we need an endpoint for that we have created `Controllers` directory


#### Entities
```cs
// Entities/User.cs
namespace JwtAuth.Entities
{
    public class User
    {
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
    }
}

```

#### Entity Model (DTO)
```cs
// Entities/Models/UserDto.cs
namespace JwtAuth.Entities.Models
{
    public class UserDto
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
```

#### Controller
```cs
// Controller/AuthController.cs
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
    }
}

```

2. Logging them in
* we need to check if either field is empty
* We need to find the user
* we need to verify the password

##### Controllers
```cs
// Controllers/AuthController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using JwtAuth.Entities;
using JwtAuth.Entities.Models;
using Microsoft.AspNetCore.Identity;

namespace JwtAuth.Controllers
{
// ....................
// ....................
// ....................
// ....................
// ....................
        [HttpPost("login")]
        public ActionResult<string> Login(UserDto request)
        {
            Console.WriteLine(request);
            // Here you would typically validate the user against a database
            if (string.IsNullOrEmpty(user.Username) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest("Username or Password cannot be empty");
            }

            if (user.Username != request.Username)
            {
                return BadRequest("User not Found");
            }


            if (new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash, request.Password) == PasswordVerificationResult.Failed)
            {
                return BadRequest("Invalid Password");
            }

            string token = "success";

            return Ok(token);
        }

        private string CreateToken(User user)
        {
            // Here you would typically create a JWT token using the user's information
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, user.Username),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.GetValue<string>("AppSettings:Token")!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            var tokenDescriptor = new JwtSecurityToken
            (
                issuer: configuration.GetValue<string>("AppSettings:Issuer"),
                audience: configuration.GetValue<string>("AppSettings:Audience"),
                expires: DateTime.Now.AddDays(1),
                claims: claims,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);

        }
}
 
```

3. Adding Roles
4. Using Refresh Tokens