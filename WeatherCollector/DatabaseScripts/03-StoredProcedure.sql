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

CREATE PROCEDURE dbo.SaveResults
	@Name NVARCHAR(255),
    @Latitude DECIMAL(10, 6),
	@Longitude DECIMAL(10, 6),
	@Temperature FLOAT,
	@ThreadName NVARCHAR(50)
AS
BEGIN
	SET NOCOUNT ON;

BEGIN TRY
BEGIN TRAN
INSERT INTO Result (Name, Latitude, Longitude, Temperature, ThreadName, CreatedAt) 
            VALUES (@Name, @Latitude, @Longitude, @Temperature, @ThreadName, GETDATE());

COMMIT TRAN;
END TRY
BEGIN CATCH
ROLLBACK TRAN;
		THROW;
END CATCH
END;