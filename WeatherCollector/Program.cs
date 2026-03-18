using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WeatherCollector.Data;
using WeatherCollector.Models;
using WeatherCollector.Services;
using WeatherCollector.Workers;
using Serilog;
using Serilog.Events;

string logDirectory = Path.Combine(AppContext.BaseDirectory, "Logs", "worker-log-.txt");

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File(logDirectory, rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    Log.Information("Logger Baslatiliyor");

    IHost host = Host.CreateDefaultBuilder(args)
        .UseSystemd()
        .UseSerilog()
        .ConfigureServices((hostContext, services) =>
        {
            services.Configure<WeatherApiConfig>(hostContext.Configuration.GetSection("WeatherApiConfig"));
            services.Configure<ThreadConfig>(hostContext.Configuration.GetSection("ThreadSettings"));

            string connectionString = hostContext.Configuration.GetConnectionString("DefaultConnection") ??
                                      "Data Source=WeatherDb.db";

            services.AddSingleton(new WeatherRepository(connectionString));

            services.AddHttpClient<OpenMeteoApiService>();
            services.AddHttpClient<WeatherApiService>();

            services.AddSingleton<Q1Worker>();
            services.AddSingleton<Q2Worker>();
        })
        .Build();

    var q1Worker = host.Services.GetRequiredService<Q1Worker>();
    var q2Worker = host.Services.GetRequiredService<Q2Worker>();

    q1Worker.Start();
    q2Worker.Start();

    Console.WriteLine("Worker Service başlatıldı.");

    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
    
