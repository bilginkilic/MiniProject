-- =============================================
-- Author:      Bilgin Kilic
-- Create date: 2024.01.17
-- Description: Müşteri no ve yetkili kontak numarası ile sirküler ve yetkili bilgilerini getirir
-- =============================================
CREATE PROCEDURE SP_SGN_GET_CUSTOMER_CIRCULAR_BY_CONTACT
    @MusteriNo varchar(20),
    @YetkiliKontaktNo varchar(20) = NULL  -- Opsiyonel, NULL ise tüm yetkililer gelir
AS
BEGIN
    SET NOCOUNT ON;

    -- Önce müşterinin sirkülerlerini kontrol et
    IF NOT EXISTS (SELECT 1 FROM SGN_CIRCULAR WHERE MusteriNo = @MusteriNo AND Durumu = 'Aktif')
    BEGIN
        SELECT 'Müşteriye ait aktif sirküler bulunamadı.' as Message;
        RETURN;
    END

    -- Sirküler ve yetkili bilgilerini getir
    SELECT 
        c.MusteriNo,
        c.SirkulerNo,
        c.SirkulerTarihi,
        c.SirkulerDurumu,
        a.YetkiliKontaktNo,
        a.YetkiliAdi,
        a.YetkiSekli,
        a.YetkiTarihi,
        a.YetkiBitisTarihi,
        a.YetkiGrubu,
        a.SinirliYetkiDetaylari,
        a.YetkiTurleri,
        a.YetkiTutari,
        a.YetkiDovizCinsi,
        a.YetkiDurumu,
        CASE 
            WHEN a.YetkiBitisTarihi = 'Aksi Karara Kadar' THEN 'Geçerli'
            WHEN CONVERT(date, a.YetkiBitisTarihi, 104) >= GETDATE() THEN 'Geçerli'
            ELSE 'Süresi Geçmiş'
        END as GecerlilikDurumu
    FROM SGN_CIRCULAR c
    INNER JOIN SGN_AUTHDETAIL a ON c.SirkulerNo = a.SirkulerNo
    WHERE c.MusteriNo = @MusteriNo
        AND c.Durumu = 'Aktif'
        AND (@YetkiliKontaktNo IS NULL OR a.YetkiliKontaktNo = @YetkiliKontaktNo)
        AND a.YetkiDurumu = 'Aktif'
    ORDER BY c.SirkulerTarihi DESC, a.YetkiTarihi DESC;

    -- Yetkili filtresine göre kayıt yoksa bilgi mesajı dön
    IF @@ROWCOUNT = 0 AND @YetkiliKontaktNo IS NOT NULL
    BEGIN
        SELECT 'Belirtilen yetkili kontak numarasına ait kayıt bulunamadı.' as Message;
        RETURN;
    END
END
GO