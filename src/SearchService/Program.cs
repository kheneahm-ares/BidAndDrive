using MassTransit;
using MongoDB.Driver;
using MongoDB.Entities;
using SearchService.Consumers;
using SearchService.Data;
using SearchService.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddMassTransit(config => 
{

    //add consumers/workers
    config.AddConsumersFromNamespaceContaining<AuctionCreatedConsumer>();

    //prefix consumer with "search"
    config.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("search", false));

    //will use localhost by default
    config.UsingRabbitMq((context, cfg) => {
        cfg.ConfigureEndpoints(context);
    });
        
});

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
