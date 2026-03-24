using Microsoft.Extensions.Options;
using WeatherCollector.Models;
using WeatherCollector.Data;
using WeatherCollector.Services;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.SignalR;
using WeatherCollector.Hubs;

namespace WeatherCollector.Workers;

public class Q2Worker
{
    private readonly WeatherRepository _weatherRepository;
    private readonly int _sleepInterval;
    private Thread _thread;
    private bool _isRunning;
    private readonly OpenMeteoApiService OpenMeteoApiService;
    private readonly ILogger<Q2Worker> _logger;
    private readonly QueueManager _queueManager;
    private readonly IHubContext<WeatherHub> _weatherHub;

    public Q2Worker (WeatherRepository weatherRepository,IOptions<ThreadConfig> threadConfig,
        OpenMeteoApiService openMeteoApiService, ILogger<Q2Worker> logger,
        QueueManager queueManager, IHubContext<WeatherHub> weatherHub)
    {
        _weatherRepository = weatherRepository;
        OpenMeteoApiService = openMeteoApiService;
        _sleepInterval = threadConfig.Value.Q2SleepTime;
        _logger = logger;
        _queueManager = queueManager;
        _weatherHub = weatherHub;
    }

    public void Start()
    {
        _isRunning = true;
        _thread = new Thread(DoWork);
        _thread.IsBackground = true;
        _thread.Name = "Q2Worker";
        _thread.Start();
    }

    public void Stop()
    {
        _isRunning = false;
        _thread.Join();
    }

    private void DoWork()
    {
        while (_isRunning)
        {
            try
            {
                var msg =  _queueManager
                    .Q2Queue
                    .Take();
                
                var values = OpenMeteoApiService
                    .GetTemperatureAsync(msg.Latitude, msg.Longitude)
                    .GetAwaiter()
                    .GetResult();

                string name = values.Item1;
                double temp = values.Item2;
                
                var results = new WeatherResults
                {
                    Name = name,
                    Longitude = msg.Longitude,
                    Latitude = msg.Latitude,
                    Temperature = temp,
                    ThreadName = _thread.Name
                };
                
                _weatherRepository.SaveResults(results);
         
                _logger.LogInformation("[{ThreadName}] API'den çekildi ve veritabanına yazıldı: {CityName} ({Latitude}, {Longitude}) -> Sıcaklık: {Temperature}°C", 
                    _thread.Name, name, msg.Latitude, msg.Longitude, temp);
                
                _weatherHub.Clients.All.SendAsync("ReceiveWeatherResult", new 
                {
                    lat = msg.Latitude,
                    lon = msg.Longitude,
                    cityName = name,
                    source = "Q2",
                    temperature = temp,
                    date = DateTime.Now.ToString("HH:mm:ss")
                }).GetAwaiter().GetResult();
                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{ThreadName}] Hata: {error}", _thread.Name, ex.Message);
                Thread.Sleep(_sleepInterval);
            }
        }
    }
}