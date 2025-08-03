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
dotnet list package # all packages are in `.csproj`
dotnet restore # install all packages
dotnet build # build project

openssl rand -base64 64 # to generate base 64 secret to use in the appsettings.json for generation of jwt tokens

dotnet tool install --global dotnet-ef
dotnet ef migrations add Initial
dotnet ef database update

```

* Create a SQL server in docker
```sh
docker run -e "ACCEPT_EULA=Y" -e 'SA_PASSWORD=StrongP@ssw0rd!' --platform linux/amd64 -p 1433:1433 --name sqlserver -d mcr.microsoft.com/mssql/server:2022-latest
```

* The use this in the `appsettings.json`
```json
{

"ConnectionStrings": {
  "DefaultConnection": "Server=localhost,1433;Database=JwtAuthDb;User Id=sa;Password=StrongP@ssw0rd!;TrustServerCertificate=True;"
},
  "AppSettings": {
    "Token": "uf/UXIEkwBTLZTHZQqBTTWHxZj4g2WW8KeAEYMZFhKBIZSiyCo1283zg1tneGTPb3B4N1JkG8/JkHjp+u1zgqg==",
    "Issuer": "haris18896",
    "Audience": "haris18896"
  }
}
```

```cs
// .........
// 🔧 Add controller support
builder.Services.AddControllers();

// 🔧 Add OpenAPI support
builder.Services.AddOpenApi();  // you can add version param to this as well e.g // 🔧 builder.Services.AddOpenApi("v2");
builder.Services.AddDbContext<UserDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))); // database connection
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

dotnet add package Microsoft.EntityFrameworkCore.Tools
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer

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


### Registration & Login With Database
* Until now what we have done above is not connected with database, we will need Services to properly connect with the database and then fetch the user from there
* for that we need the `IAuthService` and `AuthService` in the `services` directory
* We will move the above controller logics to the Service and then in the contrller we will be calling the services from database

```cs
// IAuthService
using JwtAuth.Entities;
using JwtAuth.Entities.Models;

namespace JwtAuth.Services
{
    public interface IAuthService
    {
        Task<User> RegisterAsync(UserDto request);
        Task<string> LoginAsync(UserDto request);
    }
}
```

```cs
// AuthService
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using JwtAuth.Controllers.Data;
using JwtAuth.Entities;
using JwtAuth.Entities.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace JwtAuth.Services
{
    public class AuthService(UserDbContext context, IConfiguration configuration) : IAuthService
    {
        public async Task<string?> LoginAsync(UserDto request)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);

            // Implementation for user login
            if (user is null)
            {
                return null; // User not found
            }

            if (new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash, request.Password) == PasswordVerificationResult.Failed)
            {
                return null; // Invalid Password
            }

            return CreateToken(user);
        }

        public async Task<User> RegisterAsync(UserDto request)
        {
            if (await context.Users.AnyAsync(u => u.Username == request.Username))
            {
                return null; // User already exists
            }

            var user = new User();
            // Implementation for user registration
            var hashedPassword = new PasswordHasher<User>()
             .HashPassword(user, request.Password);

            user.Username = request.Username;
            user.PasswordHash = hashedPassword;

            context.Users.Add(user);
            await context.SaveChangesAsync();

            return user;
        }

        private string CreateToken(User user)
        {
            // Here you would typically create a JWT token using the user's information
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, user.Username),
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
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
}
```

```cs
// AuthController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using JwtAuth.Entities;
using JwtAuth.Entities.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using JwtAuth.Services;

namespace JwtAuth.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<ActionResult<User>> Register(UserDto request)
        {
            var user = await authService.RegisterAsync(request);
            if (user is null)
            {
                return BadRequest("User already exists");
            }

            return Ok(user);
        }

        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(UserDto request)
        {
            var token = await authService.LoginAsync(request);
            if (token is null)
            {
                return Unauthorized("Invalid username or password");
            }

            return Ok(token);
        }

    }
}
```

* For the Autheticated Endpoints add the below code to the AuthController of that specific endpoint
```cs
// Controller
[Authorize]
        [HttpGet]
        public IActionResult AuthenticatedOnlyEndpoints()
        {
            return Ok("This endpoint is protected and requires authentication.");
        }
```

* and then in the `program.cs` we need to add builder for authentication and authorization
```cs
// Program.cs
// ..............
// ..............
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["AppSettings:Issuer"],
            ValidAudience = builder.Configuration["AppSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["AppSettings:Token"]!)),
        };
    });

// ..............
// ..............
// ..............
app.UseAuthorization();
```


3. Adding Roles
4. Using Refresh Tokens