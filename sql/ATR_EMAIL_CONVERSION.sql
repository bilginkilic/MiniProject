-- ATR tablosundaki çoklu email adreslerini ayırıp yeni kayıtlar oluşturma
-- Created: 2024.01.19
-- Author: System
-- Description: ATR tablosundaki çoklu email kayıtlarını tekil kayıtlara dönüştürme
-- Example: hhu@ho.com;kkk@sss.com -> iki ayrı kayıt olarak ekleme

BEGIN TRANSACTION;

BEGIN TRY
    -- Geçici tablo oluştur
    CREATE TABLE #TempEmails
    (
        ID INT IDENTITY(1,1),
        OriginalID INT,
        Email VARCHAR(255)
    );

    -- Çoklu email içeren kayıtları bul ve geçici tabloya ekle
    DECLARE @ID INT
    DECLARE @Email VARCHAR(MAX)
    DECLARE @StartPos INT
    DECLARE @EndPos INT
    DECLARE @SingleEmail VARCHAR(255)

    -- Aktif ve çoklu email içeren kayıtları seç
    DECLARE email_cursor CURSOR FOR
    SELECT ID, Email 
    FROM ATR 
    WHERE Email LIKE '%;%' AND Status != 'D'

    OPEN email_cursor
    FETCH NEXT FROM email_cursor INTO @ID, @Email

    WHILE @@FETCH_STATUS = 0
    BEGIN
        SET @StartPos = 1
        SET @EndPos = CHARINDEX(';', @Email)
        
        WHILE @EndPos > 0
        BEGIN
            SET @SingleEmail = LTRIM(RTRIM(SUBSTRING(@Email, @StartPos, @EndPos - @StartPos)))
            
            IF LEN(@SingleEmail) > 0
                INSERT INTO #TempEmails (OriginalID, Email)
                VALUES (@ID, @SingleEmail)
            
            SET @StartPos = @EndPos + 1
            SET @EndPos = CHARINDEX(';', @Email, @StartPos)
        END
        
        -- Son email'i ekle
        SET @SingleEmail = LTRIM(RTRIM(SUBSTRING(@Email, @StartPos, LEN(@Email) - @StartPos + 1)))
        IF LEN(@SingleEmail) > 0
            INSERT INTO #TempEmails (OriginalID, Email)
            VALUES (@ID, @SingleEmail)
        
        FETCH NEXT FROM email_cursor INTO @ID, @Email
    END

    CLOSE email_cursor
    DEALLOCATE email_cursor;

    -- Debug için kontrol
    -- SELECT * FROM #TempEmails;

    -- Orijinal kayıtları 'D' olarak işaretle
    UPDATE ATR
    SET Status = 'D',
        UpdateDate = GETDATE(),
        UpdateUser = 'SYSTEM'
    FROM ATR
    WHERE ID IN (SELECT DISTINCT OriginalID FROM #TempEmails);

    -- Yeni kayıtları ekle
    INSERT INTO ATR (
        Email,
        Status,
        CreateDate,
        CreateUser,
        -- Diğer kolonları buraya ekle, orijinal kayıttan kopyalanacak
        [Name],
        [Description],
        -- ... diğer kolonlar
    )
    SELECT 
        t.Email,
        'A' as Status, -- Aktif
        GETDATE() as CreateDate,
        'SYSTEM' as CreateUser,
        -- Orijinal kayıttan diğer kolonları al
        a.[Name],
        a.[Description]
        -- ... diğer kolonlar
    FROM #TempEmails t
    INNER JOIN ATR a ON t.OriginalID = a.ID;

    -- Geçici tabloyu temizle
    DROP TABLE #TempEmails;

    -- İşlemi onayla
    COMMIT TRANSACTION;

    -- Sonuçları göster
    SELECT 'Conversion completed successfully' as Result;

END TRY
BEGIN CATCH
    -- Hata durumunda geri al
    ROLLBACK TRANSACTION;
    
    SELECT 
        ERROR_NUMBER() as ErrorNumber,
        ERROR_MESSAGE() as ErrorMessage,
        ERROR_LINE() as ErrorLine,
        ERROR_SEVERITY() as ErrorSeverity,
        ERROR_STATE() as ErrorState;
END CATCH;
