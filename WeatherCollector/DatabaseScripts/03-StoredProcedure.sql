CREATE PROCEDURE dbo.GetCoordinates
    @TableName NVARCHAR(50)
AS
BEGIN

	IF @TableName = 'q1'
BEGIN
SELECT TOP 1 Id, Latitude, Longitude FROM q1
END
ELSE IF @TableName ='q2'
BEGIN
SELECT TOP 1 Id, Latitude, Longitude FROM q2
END
ELSE
BEGIN
        THROW 50000, 'Gecersiz tablo adi parametresi gonderildi!', 1;
END

END;

GO

CREATE PROCEDURE dbo.SaveResultsAndDelete
    @TableName NVARCHAR(50),
	@Id INT,
	@Name NVARCHAR(255),
    @Latitude DECIMAL(10, 6),
	@Longitude DECIMAL(10, 6),
	@Temperature FLOAT,
	@ThreadName NVARCHAR(50)
AS
BEGIN
BEGIN TRY
BEGIN TRAN
INSERT INTO Result (Name, Latitude, Longitude, Temperature, ThreadName, CreatedAt) 
            VALUES (@Name, @Latitude, @Longitude, @Temperature, @ThreadName, GETDATE());
			
            IF @TableName = 'q1'
BEGIN
DELETE FROM q1 WHERE Id = @Id;
END
ELSE IF @TableName = 'q2'
BEGIN
DELETE FROM q2 WHERE Id = @Id;
END
ELSE
BEGIN
                THROW 50000, 'Gecersiz tablo adi parametresi gonderildi!', 1;
END

COMMIT TRAN;
END TRY
BEGIN CATCH
ROLLBACK TRAN;
		THROW;
END CATCH
END;