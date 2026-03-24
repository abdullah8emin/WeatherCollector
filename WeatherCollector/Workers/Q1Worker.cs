using System.Threading.Channels;
using Microsoft.Extensions.Options;
using WeatherCollector.Models;
using WeatherCollector.Data;
using WeatherCollector.Services;
using Microsoft.Extensions.Logging;

namespace WeatherCollector.Workers;

public class Q1Worker
{
    private readonly WeatherRepository _weatherRepository;
    private readonly int _sleepInterval;
    private Thread _thread;
    private bool _isRunning;
    private readonly WeatherApiService _weatherApiService;
    private readonly ILogger<Q1Worker> _logger;
    private readonly QueueManager _queueManager;
    
    public Q1Worker (WeatherRepository weatherRepository, IOptions<ThreadConfig> threadConfig, WeatherApiService weatherApiService, ILogger<Q1Worker> logger, QueueManager queueManager)
    {
        _weatherRepository = weatherRepository;
        _weatherApiService = weatherApiService;
        _sleepInterval = threadConfig.Value.Q1SleepTime;
        _logger = logger;
        _queueManager = queueManager;
    }

    public void Start()
    {
        _isRunning = true;
        _thread = new Thread(DoWork);
        _thread.IsBackground = true;
        _thread.Name = "Q1Worker";
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
                
                var msg = _queueManager
                    .Q1Queue
                    .Take();    
                
                var values = _weatherApiService
                    .GetTemperatureAsync(msg.Latitude, msg.Longitude)
                    .GetAwaiter()
                    .GetResult();

                string name = values.Item1;
                double temp = values.Item2;
                
                var results = new WeatherResults
                {
                    Name = name,
                    Latitude = msg.Latitude,
                    Longitude = msg.Longitude,
                    Temperature = temp,
                    ThreadName = _thread.Name
                };
                
                _weatherRepository.SaveResults(results);
                    
                _logger.LogInformation("[{ThreadName}] API'den çekildi ve veritabanına yazıldı: {CityName} ({Latitude}, {Longitude}) -> Sıcaklık: {Temperature}°C", 
                    _thread.Name, name, msg.Latitude, msg.Longitude, temp);
                    
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{ThreadName}] Hata: {error}", _thread.Name, ex.Message);
                Thread.Sleep(_sleepInterval);
            }
        }
    }
}