using Microsoft.Extensions.Options;
using WeatherCollector.Models;
using WeatherCollector.Data;
using WeatherCollector.Services;

namespace WeatherCollector.Workers;

public class Q1Worker
{
    private readonly WeatherRepository _weatherRepository;
    private readonly int _sleepInterval;
    private Thread _thread;
    private bool _isRunning;
    private readonly WeatherApiService _weatherApiService;
    
    public Q1Worker (WeatherRepository weatherRepository, IOptions<ThreadConfig> threadConfig, WeatherApiService weatherApiService)
    {
        _weatherRepository = weatherRepository;
        _weatherApiService = weatherApiService;
        _sleepInterval = threadConfig.Value.Q1SleepTime;
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
             
                    
              Console.WriteLine($"[{_thread.Name}] API'den çekildi ve veritabanına yazıldı: {name} ({coord.Latitude}, {coord.Longitude}) -> Sıcaklık: {temp}°C");
                }
                else 
                {
                    Console.WriteLine($"{_thread.Name} islenecek veri bulunamadi");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{_thread.Name} hata: {ex.Message}");
            }

            Thread.Sleep(_sleepInterval);
        }
    }
}