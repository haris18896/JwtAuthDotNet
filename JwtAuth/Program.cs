using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// 🔧 Add controller support
builder.Services.AddControllers();

// 🔧 Add OpenAPI support
builder.Services.AddOpenApi();

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
