using EventBus.Messages.Common;
using MassTransit;
using Ordering.API.EventBusConsumer;
using Ordering.API.Extensions;
using Ordering.API.Mapping;
using Ordering.Application;
using Ordering.Infrastructure;
using Ordering.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Application and Infrastructure services.
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureService(builder.Configuration);

// MassTransit-RabbitMQ Configuration
// And join as a consumer subscriber to the BasketCheckoutQueue
builder.Services.AddMassTransit(config =>
{
  config.AddConsumer<BasketCheckoutConsumer>();
  config.UsingRabbitMq((ctx, cfg) =>
  {
    cfg.Host(builder.Configuration["EventBusSettings:HostAddress"]);
    cfg.ReceiveEndpoint(EventBusConstants.BasketCheckoutQueue, c =>
    {
      c.ConfigureConsumer<BasketCheckoutConsumer>(ctx);
    });
  });
});

//Add automapper
builder.Services.AddAutoMapper(typeof(OrderingProfile));

// Add the consumer as a scoped service so it can be injected into the controller
builder.Services.AddScoped<BasketCheckoutConsumer>();

var app = builder.Build();


// Configure the HTTP request pipeline.
// Print the environment name to the console.
Console.WriteLine($"Environment: {app.Environment.EnvironmentName}");
if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Migrate any database changes on startup (includes initial db creation)
app.MigrateDatabase<OrderContext>((context, services) =>
    {
      var logger = services.GetService<ILogger<OrderContextSeed>>();
      OrderContextSeed
          .SeedAsync(context, logger)
          .Wait();
    })
    .Run();
