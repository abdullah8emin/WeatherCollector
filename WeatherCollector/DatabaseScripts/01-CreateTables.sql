01-CreateTables.sql

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='q1' and xtype='U')
CREATE TABLE q1 (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Latitude FLOAT NOT NULL,
    Longitude FLOAT NOT NULL);
go

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='q2' and xtype='U')
CREATE TABLE q2 (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Latitude FLOAT NOT NULL,
    Longitude FLOAT NOT NULL);
go

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Result' and xtype='U')
CREATE TABLE Result (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(255) NOT NULL,
    Latitude FLOAT NOT NULL,
    Longitude FLOAT NOT NULL,
    Temperature FLOAT NOT NULL,
    ThreadName NVARCHAR(50) NOT NULL,
    CreatedAt DATETIME NOT NULL
