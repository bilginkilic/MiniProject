-- Drop existing procedures
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_SGN_AUTHDETAIL_INSERT')
    DROP PROCEDURE [dbo].[SP_SGN_AUTHDETAIL_INSERT]
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_SGN_AUTHDETAIL_UPDATE')
    DROP PROCEDURE [dbo].[SP_SGN_AUTHDETAIL_UPDATE]
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_SGN_AUTHDETAIL_SELECT_BY_ID')
    DROP PROCEDURE [dbo].[SP_SGN_AUTHDETAIL_SELECT_BY_ID]
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_SGN_AUTHDETAIL_SELECT_BY_CIRCULAR')
    DROP PROCEDURE [dbo].[SP_SGN_AUTHDETAIL_SELECT_BY_CIRCULAR]
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_SGN_AUTHDETAIL_SEARCH')
    DROP PROCEDURE [dbo].[SP_SGN_AUTHDETAIL_SEARCH]
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_SGN_AUTHDETAIL_GET_ALL_ACTIVE')
    DROP PROCEDURE [dbo].[SP_SGN_AUTHDETAIL_GET_ALL_ACTIVE]
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_SGN_AUTHDETAIL_GET_WITH_SIGNATURES')
    DROP PROCEDURE [dbo].[SP_SGN_AUTHDETAIL_GET_WITH_SIGNATURES]
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_SGN_AUTHDETAIL_SIGNATURES_INSERT')
    DROP PROCEDURE [dbo].[SP_SGN_AUTHDETAIL_SIGNATURES_INSERT]
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_SGN_AUTHDETAIL_SIGNATURES_UPDATE')
    DROP PROCEDURE [dbo].[SP_SGN_AUTHDETAIL_SIGNATURES_UPDATE]
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_SGN_AUTHDETAIL_SIGNATURES_SELECT_BY_ID')
    DROP PROCEDURE [dbo].[SP_SGN_AUTHDETAIL_SIGNATURES_SELECT_BY_ID]
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_SGN_AUTHDETAIL_SIGNATURES_SELECT_BY_AUTHDETAIL')
    DROP PROCEDURE [dbo].[SP_SGN_AUTHDETAIL_SIGNATURES_SELECT_BY_AUTHDETAIL]
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_SGN_CIRCULAR_INS_SP')
    DROP PROCEDURE [dbo].[SP_SGN_CIRCULAR_INS_SP]
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_SGN_CIRCULAR_UPD_SP')
    DROP PROCEDURE [dbo].[SP_SGN_CIRCULAR_UPD_SP]
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_SGN_CIRCULAR_SEL_SP')
    DROP PROCEDURE [dbo].[SP_SGN_CIRCULAR_SEL_SP]
GO

-- Create new procedures
CREATE PROCEDURE [dbo].[SGN_AUTHDETAIL_INSERT]
    @CustomerNo INT NULL,
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
        [CustomerNo], [YetkiliKontakt], [YetkiliAdi], [YetkiSekli],
        [YetkiTarihi], [YetkiBitisTarihi], [YetkiGrubu], [SinirliYetkiDetaylari],
        [YetkiTurleri], [YetkiTutari], [YetkiDovizCinsi], [YetkiDurumu],
        [CreateDate], [LastUpdate], [RecordStatus]
    )
    VALUES
    (
        @CustomerNo, @YetkiliKontakt, @YetkiliAdi, @YetkiSekli,
        @YetkiTarihi, @YetkiBitisTarihi, @YetkiGrubu, @SinirliYetkiDetaylari,
        @YetkiTurleri, @YetkiTutari, @YetkiDovizCinsi, @YetkiDurumu,
        GETDATE(), GETDATE(), 'A'
    );
    
    SELECT SCOPE_IDENTITY() AS ID;
END;
GO

CREATE PROCEDURE [dbo].[SGN_AUTHDETAIL_UPDATE]
    @ID INT,
    @CustomerNo INT,
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
        [CustomerNo] = @CustomerNo,
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

CREATE PROCEDURE [dbo].[SGN_AUTHDETAIL_SELECT_BY_ID]
    @ID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        [ID], [CustomerNo], [YetkiliKontakt], [YetkiliAdi],
        [YetkiSekli], [YetkiTarihi], [YetkiBitisTarihi], [YetkiGrubu],
        [SinirliYetkiDetaylari], [YetkiTurleri], [YetkiTutari],
        [YetkiDovizCinsi], [YetkiDurumu], [CreateDate], [LastUpdate], [RecordStatus]
    FROM [dbo].[SGN_AUTHDETAIL]
    WHERE [ID] = @ID AND [RecordStatus] = 'A';
END;
GO

CREATE PROCEDURE [dbo].[SGN_AUTHDETAIL_SELECT_BY_CIRCULAR]
    @CircularID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        [ID], [CustomerNo], [YetkiliKontakt], [YetkiliAdi],
        [YetkiSekli], [YetkiTarihi], [YetkiBitisTarihi], [YetkiGrubu],
        [SinirliYetkiDetaylari], [YetkiTurleri], [YetkiTutari],
        [YetkiDovizCinsi], [YetkiDurumu], [CreateDate], [LastUpdate], [RecordStatus]
    FROM [dbo].[SGN_AUTHDETAIL]
    WHERE [CircularID] = @CircularID AND [RecordStatus] = 'A'
    ORDER BY [YetkiliAdi];
END;
GO

CREATE PROCEDURE [dbo].[SGN_AUTHDETAIL_SEARCH]
    @YetkiliAdi NVARCHAR(200) = NULL,
    @YetkiGrubu NVARCHAR(100) = NULL,
    @YetkiDurumu NVARCHAR(50) = NULL,
    @BaslangicTarihi DATE = NULL,
    @BitisTarihi DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        [ID], [CustomerNo], [YetkiliKontakt], [YetkiliAdi],
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

CREATE PROCEDURE [dbo].[SGN_AUTHDETAIL_GET_ALL_ACTIVE]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        [ID], [CustomerNo], [YetkiliKontakt], [YetkiliAdi],
        [YetkiSekli], [YetkiTarihi], [YetkiBitisTarihi], [YetkiGrubu],
        [SinirliYetkiDetaylari], [YetkiTurleri], [YetkiTutari],
        [YetkiDovizCinsi], [YetkiDurumu], [CreateDate], [LastUpdate], [RecordStatus]
    FROM [dbo].[SGN_AUTHDETAIL]
    WHERE [RecordStatus] = 'A'
    ORDER BY [YetkiliAdi];
END;
GO

CREATE PROCEDURE [dbo].[SGN_AUTHDETAIL_GET_WITH_SIGNATURES]
    @ID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Get yetkili details
    SELECT 
        [ID], [CustomerNo], [YetkiliKontakt], [YetkiliAdi],
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

CREATE PROCEDURE [dbo].[SGN_AUTHDETAIL_SIGNATURES_INSERT]
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

CREATE PROCEDURE [dbo].[SGN_AUTHDETAIL_SIGNATURES_UPDATE]
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

CREATE PROCEDURE [dbo].[SGN_AUTHDETAIL_SIGNATURES_SELECT_BY_ID]
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

CREATE PROCEDURE [dbo].[SGN_AUTHDETAIL_SIGNATURES_SELECT_BY_AUTHDETAIL]
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

CREATE PROCEDURE [dbo].[SGN_CIRCULAR_INS]
    @CustomerNo INT NULL,
    @CompanyTitle nvarchar(max) NULL,
    @IssuedDate date NULL,
    @ValidityDate date NULL,
    @InternalBylawsTsgDate date NULL,
    @SpecialCases nvarchar(max) NULL,
    @CircularType nvarchar(max) NULL,
    @CircularNotaryNo nvarchar(max) NULL,
    @Description nvarchar(max) NULL,
    @CircularStatus nvarchar(max) NULL,
    @IsABoardOfDirectorsDecisionRequired nvarchar(50) NULL,
    @MainCircularDate date NULL,
    @MainCircularRef nvarchar(50) NULL,
    @AdditionalDocuments nvarchar(max) NULL,
    @Channel nvarchar(max) NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    INSERT INTO [dbo].[SGN_CIRCULAR]
    (
        [CustomerNo], [CompanyTitle], [IssuedDate], [ValidityDate],
        [InternalBylawsTsgDate], [SpecialCases], [CircularType], [CircularNotaryNo],
        [Description], [CircularStatus], [IsABoardOfDirectorsDecisionRequired],
        [MainCircularDate], [MainCircularRef], [AdditionalDocuments],
        [Channel], [CreateDate], [LastUpdate], [RecordStatus]
    )
    VALUES
    (
        @CustomerNo, @CompanyTitle, @IssuedDate, @ValidityDate,
        @InternalBylawsTsgDate, @SpecialCases, @CircularType, @CircularNotaryNo,
        @Description, @CircularStatus, @IsABoardOfDirectorsDecisionRequired,
        @MainCircularDate, @MainCircularRef, @AdditionalDocuments,
        @Channel, GETDATE(), GETDATE(), 'A'
    );

    SELECT SCOPE_IDENTITY() AS ID;
END;
GO

CREATE PROCEDURE [dbo].[SGN_CIRCULAR_UPD]
    @ID int,
    @CustomerNo INT NULL,
    @CompanyTitle nvarchar(max) NULL,
    @IssuedDate date NULL,
    @ValidityDate date NULL,
    @InternalBylawsTsgDate date NULL,
    @SpecialCases nvarchar(max) NULL,
    @CircularType nvarchar(max) NULL,
    @CircularNotaryNo nvarchar(max) NULL,
    @Description nvarchar(max) NULL,
    @CircularStatus nvarchar(max) NULL,
    @IsABoardOfDirectorsDecisionRequired nvarchar(50) NULL,
    @MainCircularDate date NULL,
    @MainCircularRef nvarchar(50) NULL,
    @AdditionalDocuments nvarchar(max) NULL,
    @Channel nvarchar(max) NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    UPDATE [dbo].[SGN_CIRCULAR]
    SET
        [CustomerNo] = @CustomerNo,
        [CompanyTitle] = @CompanyTitle,
        [IssuedDate] = @IssuedDate,
        [ValidityDate] = @ValidityDate,
        [InternalBylawsTsgDate] = @InternalBylawsTsgDate,
        [SpecialCases] = @SpecialCases,
        [CircularType] = @CircularType,
        [CircularNotaryNo] = @CircularNotaryNo,
        [Description] = @Description,
        [CircularStatus] = @CircularStatus,
        [IsABoardOfDirectorsDecisionRequired] = @IsABoardOfDirectorsDecisionRequired,
        [MainCircularDate] = @MainCircularDate,
        [MainCircularRef] = @MainCircularRef,
        [AdditionalDocuments] = @AdditionalDocuments,
        [Channel] = @Channel,
        [LastUpdate] = GETDATE()
    WHERE [ID] = @ID AND [RecordStatus] = 'A';
END;
GO

CREATE PROCEDURE [dbo].[SGN_CIRCULAR_SEL]
    @ID int
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    SELECT 
        [ID], [CustomerNo], [CompanyTitle], [IssuedDate], [ValidityDate],
        [InternalBylawsTsgDate], [SpecialCases], [CircularType], [CircularNotaryNo],
        [Description], [CircularStatus], [IsABoardOfDirectorsDecisionRequired],
        [MainCircularDate], [MainCircularRef], [AdditionalDocuments],
        [RecordStatus], [CreateDate], [LastUpdate], [Channel],
        (SELECT COUNT(1) FROM [dbo].[SGN_AUTHDETAIL] a 
         WHERE a.[CircularID] = c.[ID] AND a.[RecordStatus] = 'A') as YetkiliCount
    FROM [dbo].[SGN_CIRCULAR] c
    WHERE [ID] = @ID AND [RecordStatus] = 'A';
END;
GO
