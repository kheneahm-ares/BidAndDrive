using Data.AuctionService;
using Entities.AuctionService;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddDbContext<AuctionDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddMassTransit(config => 
{
    //outbox
    config.AddEntityFrameworkOutbox<AuctionDbContext>(
        o => {
            //look into outbox every 10 seconds and deliver if any
            o.QueryDelay = TimeSpan.FromSeconds(10);

            o.UsePostgres();
            o.UseBusOutbox();
        }
    );

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
    DbSeed.Seed(app);
}
catch
{
    Console.WriteLine("Could not seed database");
}

app.Run();
