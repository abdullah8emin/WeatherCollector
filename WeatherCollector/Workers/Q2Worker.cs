using Microsoft.Extensions.Options;
using WeatherCollector.Models;
using WeatherCollector.Data;
using WeatherCollector.Services;
using Microsoft.Extensions.Logging;

namespace WeatherCollector.Workers;

public class Q2Worker
{
    private readonly WeatherRepository _weatherRepository;
    private readonly int _sleepInterval;
    private Thread _thread;
    private bool _isRunning;
    private readonly OpenMeteoApiService OpenMeteoApiService;
    private readonly ILogger<Q2Worker> _logger;

    public Q2Worker (WeatherRepository weatherRepository, IOptions<ThreadConfig> threadConfig,  OpenMeteoApiService openMeteoApiService, ILogger<Q2Worker> logger)
    {
        _weatherRepository = weatherRepository;
        OpenMeteoApiService = openMeteoApiService;
        _sleepInterval = threadConfig.Value.Q2SleepTime;
        _logger = logger;
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
    }

    private void DoWork()
    {
        while (_isRunning)
        {
            try
            {
                var coord = _weatherRepository.GetCoordinates("q2").FirstOrDefault();

                if (coord != null)
                {
                    var values = OpenMeteoApiService.GetTemperatureAsync(coord.Latitude, coord.Longitude).GetAwaiter().GetResult();

                    string name = values.Item1;
                    double temp = values.Item2;
                
                    var results = new WeatherResults
                    {
                        Name = name,
                        Longitude = coord.Longitude,
                        Latitude = coord.Latitude,
                        Temperature = temp,
                        ThreadName = _thread.Name,
                        CreatedAt = DateTime.Now
                    };
            
                    _weatherRepository.SaveResults(results);
                    _weatherRepository.DeleteCoordinates("q2", coord.Id);
         
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