using Marten;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.AddServiceDefaults();

builder.AddNpgsqlDataSource("banking");

builder.Services.AddMarten(options =>
{

}).UseNpgsqlDataSource().UseLightweightSessions();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.MapGet("/users", async (IQuerySession session) =>
{
    var users = await session.Query<User>().ToListAsync();
    return TypedResults.Ok(new { users });
});

app.Run();

public record User(Guid Id, string Name);

public partial class Program { }