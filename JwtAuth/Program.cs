using JwtAuth.Controllers.Data;
using JwtAuth.Services;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// 🔧 Add controller support
builder.Services.AddControllers();

// 🔧 Add OpenAPI support
builder.Services.AddOpenApi();

builder.Services.AddDbContext<UserDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IAuthService, AuthService>();

var app = builder.Build();

// Configure middleware and endpoints
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();

// 🔧 Register controller endpoints
app.MapControllers();

app.Run();
