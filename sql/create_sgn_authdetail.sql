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
