using Ordering.API.Extensions;
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
