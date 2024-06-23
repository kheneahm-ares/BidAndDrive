using MongoDB.Driver;
using MongoDB.Entities;
using SearchService.Data;
using SearchService.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var app = builder.Build();

app.UseAuthorization();

app.MapControllers();

try
{
    await DbInitializer.InitAsync(app);
}
catch
{
    Console.WriteLine("Could not seed database");
}

app.Run();
