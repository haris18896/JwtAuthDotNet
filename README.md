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

```

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
3. Adding Roles
4. Using Refresh Tokens