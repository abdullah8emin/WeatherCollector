using System.Collections.Concurrent;

namespace WeatherCollector;

public class CoordinateMessage
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

public class QueueManager
{
    public BlockingCollection<CoordinateMessage> Q1Queue { get; } = new BlockingCollection<CoordinateMessage>();
    public BlockingCollection<CoordinateMessage> Q2Queue { get; } = new BlockingCollection<CoordinateMessage>();
}
