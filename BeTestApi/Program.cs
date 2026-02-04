var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("AllowAll");

// Simple health check
app.MapGet("/api/health", () => new { Status = "OK", Message = "Backend is running!" })
   .WithName("HealthCheck")
   .WithOpenApi();

// Simple greeting API
app.MapGet("/api/greeting", (string? name) =>
{
    var greeting = string.IsNullOrEmpty(name)
        ? "Hello from Backend!"
        : $"Hello, {name}! Welcome from Backend API.";
    return new { Greeting = greeting, Timestamp = DateTime.UtcNow };
})
.WithName("GetGreeting")
.WithOpenApi();

app.Run();
