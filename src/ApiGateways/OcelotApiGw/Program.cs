using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Cache.CacheManager;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;


var builder = WebApplication.CreateBuilder(args);

IConfiguration configuration = new ConfigurationBuilder()
                            .AddJsonFile($"Ocelot.{builder.Environment.EnvironmentName}.json")
                            .Build();

builder.Services.AddOcelot(configuration).AddCacheManager(x => x.WithDictionaryHandle());


builder.Services.AddLogging(logging =>
{
  logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
  logging.AddConsole();
  logging.AddDebug();
});



var app = builder.Build();

await app.UseOcelot();


app.MapGet("/", () => "Hello World!");
app.Run();
