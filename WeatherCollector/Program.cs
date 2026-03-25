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
using WeatherCollector.Hubs;
using System.Globalization;

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

    builder.Services.AddSignalR();
    
    var app = builder.Build();

    app.UseDefaultFiles();
    app.UseStaticFiles();
    
    app.MapHub<WeatherHub>("/WeatherHub");
    
    app.MapPost("/api/coordinates", (CoordinateMessage msg, QueueManager manager) =>
    {
        manager.Q1Queue.Add(msg);
        manager.Q2Queue.Add(msg);
    
        Log.Information("Arayüzden veri geldi: {Lat}, {Lon}", msg.Latitude, msg.Longitude);
        return Results.Ok(new { message = "Koordinatlar başarıyla workerlara gönderildi!" });
    });


    app.MapPost("/api/upload-csv", async (IFormFile file, QueueManager manager) =>
    {
        if (file == null || file.Length == 0) return Results.BadRequest("Dosya boş.");

        int count = 0;
        using var reader = new StreamReader(file.OpenReadStream());
    
        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(line)) continue;

            var parts = line.Split(',');

            if (parts.Length >= 2)
            {
                bool isLatValid = double.TryParse(parts[0].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out double lat);
                bool isLonValid = double.TryParse(parts[1].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out double lon);

                if (isLatValid && isLonValid)
                {
                    var msg = new CoordinateMessage { Latitude = lat, Longitude = lon };
                    manager.Q1Queue.Add(msg);
                    manager.Q2Queue.Add(msg);
                    count++;
                }
            }
        }

        Log.Information("CSV yüklendi. Toplam: {Count} kayıt.", count);
        return Results.Ok(new { message = $"{count} adet lokasyon başarıyla workerlara gönderildi!" });
    }).DisableAntiforgery();

    var q1Worker = app.Services.GetRequiredService<Q1Worker>();
    var q2Worker = app.Services.GetRequiredService<Q2Worker>();

    q1Worker.Start();
    q2Worker.Start();

    Console.WriteLine("Web API ve Worker Service başlatıldı.");

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
