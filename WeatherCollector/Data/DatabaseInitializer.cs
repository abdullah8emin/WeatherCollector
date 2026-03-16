using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace WeatherCollector.Data;

public class DatabaseInitializer
{
    private readonly string _connectionString;
    
    public DatabaseInitializer(string connectionString)
    {
        _connectionString = connectionString;
    }

    public void InitializeDatabase()
    {
        using (IDbConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            
            string q1Table = @"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='q1' and xtype='U')
                CREATE TABLE q1 (
                    Id INT IDENTITY(1,1) PRIMARY KEY,
                    Latitude FLOAT NOT NULL,
                    Longitude FLOAT NOT NULL
                )";
            connection.Execute(q1Table);
            
            string q2Table = @"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='q2' and xtype='U')
                CREATE TABLE q2 (
                    Id INT IDENTITY(1,1) PRIMARY KEY,
                    Latitude FLOAT NOT NULL,
                    Longitude FLOAT NOT NULL
                )";
            connection.Execute(q2Table);

            string resultTable = @"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Result' and xtype='U')
                CREATE TABLE Result (
                    Id INT IDENTITY(1,1) PRIMARY KEY,
                    Name NVARCHAR(255) NOT NULL,
                    Latitude FLOAT NOT NULL,
                    Longitude FLOAT NOT NULL,
                    Temperature FLOAT NOT NULL,
                    ThreadName NVARCHAR(50) NOT NULL,
                    CreatedAt DATETIME NOT NULL
                )";
            connection.Execute(resultTable);
            
            SeedData(connection);
        }
    }
    
    private void SeedData(IDbConnection connection)
    {
        int q1Count = connection.QuerySingle<int>("SELECT COUNT(*) FROM q1");
        if (q1Count == 0)
        {
            string insertQ1 = @"
                    INSERT INTO q1 (Latitude, Longitude) VALUES 
                    (37.8380, 27.8456), 
                    (41.0082, 28.9784), 
                    (39.9208, 32.8541),
                    (38.4192, 27.1287),
                    (36.8969, 30.7133),
                    (40.1826, 29.0665),
                    (37.0000, 35.3213),
                    (41.0027, 39.7168),
                    (37.0662, 37.3833),
                    (37.8746, 32.4932);";
            connection.Execute(insertQ1);
        }

        int q2Count = connection.QuerySingle<int>("SELECT COUNT(*) FROM q2");
        if (q2Count == 0)
        {
            string insertQ2 = @"
                    INSERT INTO q2 (Latitude, Longitude) VALUES 
                    (51.5074, -0.1278), 
                    (40.7128, -74.0060), 
                    (35.6895, 139.6917),
                    (48.8566, 2.3522),
                    (52.5200, 13.4050),
                    (-33.8688, 151.2093),
                    (55.7558, 37.6173),
                    (25.2048, 55.2708),
                    (1.3521, 103.8198),
                    (41.9028, 12.4964);";
            connection.Execute(insertQ2);
        }
    }
}