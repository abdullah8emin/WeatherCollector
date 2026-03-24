using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WeatherCollector.Data;
using WeatherCollector.Models;
using WeatherCollector.Services;
using WeatherCollector.Workers;
using Serilog;
using Serilog.Events;
using WeatherCollector;

string logDirectory = Path.Combine(AppContext.BaseDirectory, "Logs", "worker-log-.txt");

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File(logDirectory, rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    Log.Information("Logger Baslatiliyor");

    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSystemd();
    builder.Host.UseSerilog();

    builder.Services.AddSingleton<QueueManager>();
    builder.Services.Configure<WeatherApiConfig>(builder.Configuration.GetSection("WeatherApiConfig"));
    builder.Services.Configure<ThreadConfig>(builder.Configuration.GetSection("ThreadSettings"));
    
    string connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=WeatherDb.db"; 

    builder.Services.AddSingleton(new WeatherRepository(connectionString));
    builder.Services.AddHttpClient<WeatherApiService>();
    builder.Services.AddHttpClient<OpenMeteoApiService>();
    
    builder.Services.AddSingleton<Q1Worker>();
    builder.Services.AddSingleton<Q2Worker>();
    
    var app = builder.Build();

    app.UseDefaultFiles();
    app.UseStaticFiles();
    
    app.MapPost("/api/coordinates", (CoordinateMessage msg, QueueManager manager) =>
    {
        manager.Q1Queue.Add(msg);
        manager.Q2Queue.Add(msg);
    
        Log.Information("Arayüzden veri geldi ve kuyruklara eklendi: {Lat}, {Lon}", msg.Latitude, msg.Longitude);
        return Results.Ok(new { message = "Koordinatlar iki işçiye de gönderildi!" });
    });

    var q1Worker = app.Services.GetRequiredService<Q1Worker>();
    var q2Worker = app.Services.GetRequiredService<Q2Worker>();

    q1Worker.Start();
    q2Worker.Start();

    Console.WriteLine("Web API ve Worker Service başlatıldı. Tarayıcıdan arayüze erişebilirsiniz.");

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
