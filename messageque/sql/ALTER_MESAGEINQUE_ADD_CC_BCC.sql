-- Mevcut MESAGEINQUE tablosuna Cc ve Bcc sütunları ekler (tablo zaten varsa çalıştırın)
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.MESAGEINQUE') AND name = 'Cc')
BEGIN
    ALTER TABLE dbo.MESAGEINQUE ADD Cc NVARCHAR(MAX) NULL;
END
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.MESAGEINQUE') AND name = 'Bcc')
BEGIN
    ALTER TABLE dbo.MESAGEINQUE ADD Bcc NVARCHAR(MAX) NULL;
END
GO
