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

// In-memory storage for todos (for testing)
var todos = new List<TodoItem>
{
    new(1, "Learn Azure", true),
    new(2, "Deploy Backend", false),
    new(3, "Connect Frontend", false)
};
var nextId = 4;

// ========== HEALTH CHECK ==========
app.MapGet("/api/health", () => new {
    Status = "OK",
    Message = "Backend is running!",
    Timestamp = DateTime.UtcNow
})
.WithName("HealthCheck")
.WithOpenApi();

// ========== CALCULATOR ==========
app.MapGet("/api/calculate", (int a, int b, string operation) =>
{
    double result = operation.ToLower() switch
    {
        "add" => a + b,
        "subtract" => a - b,
        "multiply" => a * b,
        "divide" => b != 0 ? (double)a / b : throw new Exception("Cannot divide by zero"),
        _ => throw new Exception($"Unknown operation: {operation}")
    };
    return new { A = a, B = b, Operation = operation, Result = result };
})
.WithName("Calculate")
.WithOpenApi();

// ========== TODO LIST ==========
// Get all todos
app.MapGet("/api/todos", () => todos)
   .WithName("GetTodos")
   .WithOpenApi();

// Add new todo
app.MapPost("/api/todos", (CreateTodoRequest request) =>
{
    var todo = new TodoItem(nextId++, request.Title, false);
    todos.Add(todo);
    return Results.Created($"/api/todos/{todo.Id}", todo);
})
.WithName("CreateTodo")
.WithOpenApi();

// Toggle todo complete
app.MapPut("/api/todos/{id}/toggle", (int id) =>
{
    var todo = todos.FirstOrDefault(t => t.Id == id);
    if (todo == null) return Results.NotFound(new { Error = $"Todo {id} not found" });

    var index = todos.IndexOf(todo);
    todos[index] = todo with { IsCompleted = !todo.IsCompleted };
    return Results.Ok(todos[index]);
})
.WithName("ToggleTodo")
.WithOpenApi();

// Delete todo
app.MapDelete("/api/todos/{id}", (int id) =>
{
    var todo = todos.FirstOrDefault(t => t.Id == id);
    if (todo == null) return Results.NotFound(new { Error = $"Todo {id} not found" });

    todos.Remove(todo);
    return Results.Ok(new { Message = $"Todo '{todo.Title}' deleted" });
})
.WithName("DeleteTodo")
.WithOpenApi();

// ========== RANDOM ==========
app.MapGet("/api/random", (int? min, int? max) =>
{
    var minVal = min ?? 1;
    var maxVal = max ?? 100;
    var random = Random.Shared.Next(minVal, maxVal + 1);
    return new { Min = minVal, Max = maxVal, RandomNumber = random };
})
.WithName("GetRandom")
.WithOpenApi();

app.Run();

// Models
record TodoItem(int Id, string Title, bool IsCompleted);
record CreateTodoRequest(string Title);
