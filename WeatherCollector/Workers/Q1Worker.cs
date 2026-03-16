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
    
    public Q1Worker (WeatherRepository weatherRepository, IOptions<ThreadConfig> threadConfig, WeatherApiService weatherApiService, ILogger<Q1Worker> logger)
    {
        _weatherRepository = weatherRepository;
        _weatherApiService = weatherApiService;
        _sleepInterval = threadConfig.Value.Q1SleepTime;
        _logger = logger;
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
    }

    private void DoWork()
    {
        while (_isRunning)
        {
            try
            {
                var coord = _weatherRepository.GetCoordinates("q1").FirstOrDefault();

                if (coord != null)
                {
                    var values = _weatherApiService.GetTemperatureAsync(coord.Latitude, coord.Longitude).GetAwaiter().GetResult();

                    string name = values.Item1;
                    double temp = values.Item2;
                
                    var results = new WeatherResults
                    {
                        Name = name,
                        Latitude = coord.Latitude,
                        Longitude = coord.Longitude,
                        Temperature = temp,
                        ThreadName = _thread.Name,
                        CreatedAt = DateTime.Now
                    };
                
                    _weatherRepository.SaveResults(results);
                    _weatherRepository.DeleteCoordinates("q1", coord.Id); 
                    
                    _logger.LogInformation("[{ThreadName}] API'den çekildi ve veritabanına yazıldı: {CityName} ({Latitude}, {Longitude}) -> Sıcaklık: {Temperature}°C", 
                        _thread.Name, name, coord.Latitude, coord.Longitude, temp);
                }
                else 
                {
                    _logger.LogInformation("[{ThreadName}] islenecek veri bulunamadi", _thread.Name);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{ThreadName}] Hata: {error}", _thread.Name, ex.Message);
            }

            Thread.Sleep(_sleepInterval);
        }
    }
}