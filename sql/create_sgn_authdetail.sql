-- Create SGN_AUTHDETAIL table to store YetkiliData
CREATE TABLE [dbo].[SGN_AUTHDETAIL] (
    [ID] INT IDENTITY(1,1) PRIMARY KEY,
    [CircularID] INT NOT NULL,  -- Foreign key to SGN_CIRCULAR
    [YetkiliKontakt] NVARCHAR(100),
    [YetkiliAdi] NVARCHAR(200),
    [YetkiSekli] NVARCHAR(50),
    [YetkiTarihi] DATE,
    [YetkiBitisTarihi] DATE,
    [YetkiGrubu] NVARCHAR(100),
    [SinirliYetkiDetaylari] NVARCHAR(MAX),
    [YetkiTurleri] NVARCHAR(200),
    [YetkiTutari] DECIMAL(18,2),
    [YetkiDovizCinsi] NVARCHAR(10),
    [YetkiDurumu] NVARCHAR(50),
    [CreateDate] DATETIME DEFAULT GETDATE(),
    [LastUpdate] DATETIME DEFAULT GETDATE(),
    [RecordStatus] CHAR(1) DEFAULT 'A'  -- A: Active, P: Passive, D: Deleted
);

-- Create foreign key constraint
ALTER TABLE [dbo].[SGN_AUTHDETAIL]
ADD CONSTRAINT [FK_SGN_AUTHDETAIL_SGN_CIRCULAR]
FOREIGN KEY ([CircularID]) REFERENCES [dbo].[SGN_CIRCULAR]([ID]);

-- Create index for better query performance
CREATE INDEX [IX_SGN_AUTHDETAIL_CircularID] ON [dbo].[SGN_AUTHDETAIL]([CircularID]);

-- Create table for storing signature images
CREATE TABLE [dbo].[SGN_AUTHDETAIL_SIGNATURES] (
    [ID] INT IDENTITY(1,1) PRIMARY KEY,
    [AuthDetailID] INT NOT NULL,
    [ImageData] VARBINARY(MAX),  -- Store actual image data as binary
    [SiraNo] INT NOT NULL,
    [SourcePdfPath] NVARCHAR(500),
    [CreateDate] DATETIME DEFAULT GETDATE(),
    [LastUpdate] DATETIME DEFAULT GETDATE(),
    CONSTRAINT [FK_SGN_AUTHDETAIL_SIGNATURES_SGN_AUTHDETAIL] 
    FOREIGN KEY ([AuthDetailID]) REFERENCES [dbo].[SGN_AUTHDETAIL]([ID])
);

-- Create index for signatures
CREATE INDEX [IX_SGN_AUTHDETAIL_SIGNATURES_AuthDetailID] 
ON [dbo].[SGN_AUTHDETAIL_SIGNATURES]([AuthDetailID]);

-- Add comments for documentation
EXEC sys.sp_addextendedproperty 
    @name = N'MS_Description',
    @value = N'Stores authorized person (yetkili) details for circulars',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'SGN_AUTHDETAIL';

EXEC sys.sp_addextendedproperty 
    @name = N'MS_Description',
    @value = N'Stores signature images for authorized persons',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'SGN_AUTHDETAIL_SIGNATURES';

-- Create stored procedures for SGN_AUTHDETAIL
GO
CREATE PROCEDURE [dbo].[SP_SGN_AUTHDETAIL_INSERT]
    @CircularID INT,
    @YetkiliKontakt NVARCHAR(100),
    @YetkiliAdi NVARCHAR(200),
    @YetkiSekli NVARCHAR(50),
    @YetkiTarihi DATE,
    @YetkiBitisTarihi DATE,
    @YetkiGrubu NVARCHAR(100),
    @SinirliYetkiDetaylari NVARCHAR(MAX),
    @YetkiTurleri NVARCHAR(200),
    @YetkiTutari DECIMAL(18,2),
    @YetkiDovizCinsi NVARCHAR(10),
    @YetkiDurumu NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO [dbo].[SGN_AUTHDETAIL]
    (
        [CircularID], [YetkiliKontakt], [YetkiliAdi], [YetkiSekli],
        [YetkiTarihi], [YetkiBitisTarihi], [YetkiGrubu], [SinirliYetkiDetaylari],
        [YetkiTurleri], [YetkiTutari], [YetkiDovizCinsi], [YetkiDurumu],
        [CreateDate], [LastUpdate], [RecordStatus]
    )
    VALUES
    (
        @CircularID, @YetkiliKontakt, @YetkiliAdi, @YetkiSekli,
        @YetkiTarihi, @YetkiBitisTarihi, @YetkiGrubu, @SinirliYetkiDetaylari,
        @YetkiTurleri, @YetkiTutari, @YetkiDovizCinsi, @YetkiDurumu,
        GETDATE(), GETDATE(), 'A'
    );
    
    SELECT SCOPE_IDENTITY() AS ID;
END;
GO

CREATE PROCEDURE [dbo].[SP_SGN_AUTHDETAIL_UPDATE]
    @ID INT,
    @CircularID INT,
    @YetkiliKontakt NVARCHAR(100),
    @YetkiliAdi NVARCHAR(200),
    @YetkiSekli NVARCHAR(50),
    @YetkiTarihi DATE,
    @YetkiBitisTarihi DATE,
    @YetkiGrubu NVARCHAR(100),
    @SinirliYetkiDetaylari NVARCHAR(MAX),
    @YetkiTurleri NVARCHAR(200),
    @YetkiTutari DECIMAL(18,2),
    @YetkiDovizCinsi NVARCHAR(10),
    @YetkiDurumu NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE [dbo].[SGN_AUTHDETAIL]
    SET 
        [CircularID] = @CircularID,
        [YetkiliKontakt] = @YetkiliKontakt,
        [YetkiliAdi] = @YetkiliAdi,
        [YetkiSekli] = @YetkiSekli,
        [YetkiTarihi] = @YetkiTarihi,
        [YetkiBitisTarihi] = @YetkiBitisTarihi,
        [YetkiGrubu] = @YetkiGrubu,
        [SinirliYetkiDetaylari] = @SinirliYetkiDetaylari,
        [YetkiTurleri] = @YetkiTurleri,
        [YetkiTutari] = @YetkiTutari,
        [YetkiDovizCinsi] = @YetkiDovizCinsi,
        [YetkiDurumu] = @YetkiDurumu,
        [LastUpdate] = GETDATE()
    WHERE [ID] = @ID AND [RecordStatus] = 'A';
END;
GO

CREATE PROCEDURE [dbo].[SP_SGN_AUTHDETAIL_DELETE]
    @ID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Soft delete for SGN_AUTHDETAIL
    UPDATE [dbo].[SGN_AUTHDETAIL]
    SET [RecordStatus] = 'D',
        [LastUpdate] = GETDATE()
    WHERE [ID] = @ID AND [RecordStatus] = 'A';
    
    -- Soft delete related signatures
    UPDATE [dbo].[SGN_AUTHDETAIL_SIGNATURES]
    SET [LastUpdate] = GETDATE()
    WHERE [AuthDetailID] = @ID;
END;
GO

-- Create stored procedures for SGN_AUTHDETAIL_SIGNATURES
CREATE PROCEDURE [dbo].[SP_SGN_AUTHDETAIL_SIGNATURES_INSERT]
    @AuthDetailID INT,
    @ImageData VARBINARY(MAX),
    @SiraNo INT,
    @SourcePdfPath NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO [dbo].[SGN_AUTHDETAIL_SIGNATURES]
    (
        [AuthDetailID], [ImageData], [SiraNo], [SourcePdfPath],
        [CreateDate], [LastUpdate]
    )
    VALUES
    (
        @AuthDetailID, @ImageData, @SiraNo, @SourcePdfPath,
        GETDATE(), GETDATE()
    );
    
    SELECT SCOPE_IDENTITY() AS ID;
END;
GO

CREATE PROCEDURE [dbo].[SP_SGN_AUTHDETAIL_SIGNATURES_UPDATE]
    @ID INT,
    @AuthDetailID INT,
    @ImageData VARBINARY(MAX),
    @SiraNo INT,
    @SourcePdfPath NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE [dbo].[SGN_AUTHDETAIL_SIGNATURES]
    SET 
        [AuthDetailID] = @AuthDetailID,
        [ImageData] = @ImageData,
        [SiraNo] = @SiraNo,
        [SourcePdfPath] = @SourcePdfPath,
        [LastUpdate] = GETDATE()
    WHERE [ID] = @ID;
END;
GO

CREATE PROCEDURE [dbo].[SP_SGN_AUTHDETAIL_SIGNATURES_DELETE]
    @ID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    DELETE FROM [dbo].[SGN_AUTHDETAIL_SIGNATURES]
    WHERE [ID] = @ID;
END;
GO

-- Create SELECT procedures for SGN_AUTHDETAIL
CREATE PROCEDURE [dbo].[SP_SGN_AUTHDETAIL_SELECT_BY_ID]
    @ID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        [ID], [CircularID], [YetkiliKontakt], [YetkiliAdi],
        [YetkiSekli], [YetkiTarihi], [YetkiBitisTarihi], [YetkiGrubu],
        [SinirliYetkiDetaylari], [YetkiTurleri], [YetkiTutari],
        [YetkiDovizCinsi], [YetkiDurumu], [CreateDate], [LastUpdate], [RecordStatus]
    FROM [dbo].[SGN_AUTHDETAIL]
    WHERE [ID] = @ID AND [RecordStatus] = 'A';
END;
GO

CREATE PROCEDURE [dbo].[SP_SGN_AUTHDETAIL_SELECT_BY_CIRCULAR]
    @CircularID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        [ID], [CircularID], [YetkiliKontakt], [YetkiliAdi],
        [YetkiSekli], [YetkiTarihi], [YetkiBitisTarihi], [YetkiGrubu],
        [SinirliYetkiDetaylari], [YetkiTurleri], [YetkiTutari],
        [YetkiDovizCinsi], [YetkiDurumu], [CreateDate], [LastUpdate], [RecordStatus]
    FROM [dbo].[SGN_AUTHDETAIL]
    WHERE [CircularID] = @CircularID AND [RecordStatus] = 'A'
    ORDER BY [YetkiliAdi];
END;
GO

CREATE PROCEDURE [dbo].[SP_SGN_AUTHDETAIL_SEARCH]
    @YetkiliAdi NVARCHAR(200) = NULL,
    @YetkiGrubu NVARCHAR(100) = NULL,
    @YetkiDurumu NVARCHAR(50) = NULL,
    @BaslangicTarihi DATE = NULL,
    @BitisTarihi DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        [ID], [CircularID], [YetkiliKontakt], [YetkiliAdi],
        [YetkiSekli], [YetkiTarihi], [YetkiBitisTarihi], [YetkiGrubu],
        [SinirliYetkiDetaylari], [YetkiTurleri], [YetkiTutari],
        [YetkiDovizCinsi], [YetkiDurumu], [CreateDate], [LastUpdate], [RecordStatus]
    FROM [dbo].[SGN_AUTHDETAIL]
    WHERE [RecordStatus] = 'A'
        AND (@YetkiliAdi IS NULL OR [YetkiliAdi] LIKE '%' + @YetkiliAdi + '%')
        AND (@YetkiGrubu IS NULL OR [YetkiGrubu] = @YetkiGrubu)
        AND (@YetkiDurumu IS NULL OR [YetkiDurumu] = @YetkiDurumu)
        AND (@BaslangicTarihi IS NULL OR [YetkiTarihi] >= @BaslangicTarihi)
        AND (@BitisTarihi IS NULL OR [YetkiTarihi] <= @BitisTarihi)
    ORDER BY [YetkiliAdi];
END;
GO

-- Create SELECT procedures for SGN_AUTHDETAIL_SIGNATURES
CREATE PROCEDURE [dbo].[SP_SGN_AUTHDETAIL_SIGNATURES_SELECT_BY_ID]
    @ID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        [ID], [AuthDetailID], [ImageData], [SiraNo],
        [SourcePdfPath], [CreateDate], [LastUpdate]
    FROM [dbo].[SGN_AUTHDETAIL_SIGNATURES]
    WHERE [ID] = @ID;
END;
GO

CREATE PROCEDURE [dbo].[SP_SGN_AUTHDETAIL_SIGNATURES_SELECT_BY_AUTHDETAIL]
    @AuthDetailID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        [ID], [AuthDetailID], [ImageData], [SiraNo],
        [SourcePdfPath], [CreateDate], [LastUpdate]
    FROM [dbo].[SGN_AUTHDETAIL_SIGNATURES]
    WHERE [AuthDetailID] = @AuthDetailID
    ORDER BY [SiraNo];
END;
GO

-- Create a procedure to get complete yetkili data with signatures
CREATE PROCEDURE [dbo].[SP_SGN_AUTHDETAIL_GET_WITH_SIGNATURES]
    @ID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Get yetkili details
    SELECT 
        [ID], [CircularID], [YetkiliKontakt], [YetkiliAdi],
        [YetkiSekli], [YetkiTarihi], [YetkiBitisTarihi], [YetkiGrubu],
        [SinirliYetkiDetaylari], [YetkiTurleri], [YetkiTutari],
        [YetkiDovizCinsi], [YetkiDurumu], [CreateDate], [LastUpdate], [RecordStatus]
    FROM [dbo].[SGN_AUTHDETAIL]
    WHERE [ID] = @ID AND [RecordStatus] = 'A';
    
    -- Get associated signatures
    SELECT 
        [ID], [AuthDetailID], [ImageData], [SiraNo],
        [SourcePdfPath], [CreateDate], [LastUpdate]
    FROM [dbo].[SGN_AUTHDETAIL_SIGNATURES]
    WHERE [AuthDetailID] = @ID
    ORDER BY [SiraNo];
END;
GO
