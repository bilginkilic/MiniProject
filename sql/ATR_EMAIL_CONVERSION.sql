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
    INSERT INTO #TempEmails (OriginalID, Email)
    SELECT 
        ATR.ID,
        TRIM(value) as Email
    FROM ATR
    CROSS APPLY STRING_SPLIT(ATR.Email, ';')
    WHERE ATR.Email LIKE '%;%'
    AND ATR.Status != 'D';

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
