using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WeatherCollector.Data;
using WeatherCollector.Models;
using WeatherCollector.Services;
using WeatherCollector.Workers;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.Configure<WeatherApiConfig>(hostContext.Configuration.GetSection("WeatherApiConfig"));
        services.Configure<ThreadConfig>(hostContext.Configuration.GetSection("ThreadSettings"));

        string connectionString = hostContext.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=weatherdata.db";

        services.AddSingleton(new DatabaseInitializer(connectionString));
        services.AddSingleton(new WeatherRepository(connectionString));

        services.AddHttpClient<OpenMeteoApiService>();
        services.AddHttpClient<WeatherApiService>();

        services.AddSingleton<Q1Worker>();
        services.AddSingleton<Q2Worker>();
    })
    .Build();
    
var dbInitializer = host.Services.GetRequiredService<DatabaseInitializer>();
dbInitializer.InitializeDatabase();

var q1Worker = host.Services.GetRequiredService<Q1Worker>();
var q2Worker = host.Services.GetRequiredService<Q2Worker>();

q1Worker.Start();
q2Worker.Start();

Console.WriteLine("Worker Service başlatıldı.");

await host.RunAsync();