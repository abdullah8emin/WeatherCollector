using Dapper;
using Microsoft.Data.Sqlite;
using WeatherCollector.Models;
using System.Data;

namespace WeatherCollector.Data;

public class WeatherRepository
{
    private readonly string _connectionString;
    
    public WeatherRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IEnumerable<Coordinates> GetCoordinates(string tableName)
    {
        string query = $"SELECT Id, CAST(Latitude AS REAL) AS Latitude, CAST(Longitude AS REAL) AS Longitude FROM {tableName}";

        using (IDbConnection connection = new SqliteConnection(_connectionString))
        {
            return connection.Query<Coordinates>(query);
        }
    }

    public void SaveResults(WeatherResults results)
    {
        string query = @"
            INSERT INTO Result (Name, Latitude, Longitude, Temperature, ThreadName, CreatedAt) 
                VALUES (@Name, @Latitude, @Longitude, @Temperature, @ThreadName, @CreatedAt)";
        using (IDbConnection connection = new SqliteConnection(_connectionString))
        {
            connection.Execute(query, results);
        }
    }

    public void DeleteCoordinates(string tableName, int id)
    {
        using (IDbConnection connection = new SqliteConnection(_connectionString))
        {
            connection.Open();
            connection.Execute($"DELETE FROM {tableName} WHERE Id = @id", new { id });
        }
    }
}