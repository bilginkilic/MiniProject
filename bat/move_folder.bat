@echo off
REM Klasör Kopyalama ve Kaynak Silme Scripti
REM Parametreler dosyada gömülü olarak tanımlanmıştır
REM İşlem: Kopyala → Kaynak klasörü sil

setlocal enabledelayedexpansion

REM ========================================
REM PARAMETRELER - Burayı ihtiyacınıza göre düzenleyin
REM ========================================
set "SOURCE_FOLDER=C:\temp\eski"
set "TARGET_FOLDER=C:\temp\yeni"
REM ========================================

echo.
echo ========================================
echo KLASÖR KOPYALAMA SCRIPTI
echo ========================================
echo Kaynak: %SOURCE_FOLDER%
echo Hedef:  %TARGET_FOLDER%
echo ========================================
echo.

REM Kaynak klasörün varlığını kontrol et
if not exist "%SOURCE_FOLDER%" (
    echo HATA: Kaynak klasör bulunamadı: %SOURCE_FOLDER%
    pause
    exit /b 1
)

REM Hedef klasörün parent dizininin varlığını kontrol et
for %%F in ("%TARGET_FOLDER%") do set "TARGET_PARENT=%%~dpF"
if not exist "%TARGET_PARENT%" (
    echo HATA: Hedef klasörün parent dizini bulunamadı: %TARGET_PARENT%
    pause
    exit /b 1
)

REM Hedef klasör zaten varsa kontrol et
if exist "%TARGET_FOLDER%" (
    echo UYARI: Hedef klasör zaten mevcut: %TARGET_FOLDER%
    echo Hedef klasörün içeriği üzerine yazılacak!
    set /p "OVERWRITE=Devam etmek istiyor musunuz? (E/H): "
    if /i not "!OVERWRITE!"=="E" (
        echo İşlem iptal edildi.
        pause
        exit /b 0
    )
    echo Hedef klasör siliniyor...
    rmdir /s /q "%TARGET_FOLDER%"
)

echo.
echo ========================================
echo KLASÖR KOPYALAMA İŞLEMİ BAŞLIYOR
echo ========================================
echo Kaynak: %SOURCE_FOLDER%
echo Hedef:  %TARGET_FOLDER%
echo ========================================
echo.

REM Kullanıcıdan onay al
set /p "CONFIRM=İşlemi devam ettirmek istiyor musunuz? (E/H): "
if /i not "%CONFIRM%"=="E" (
    echo İşlem iptal edildi.
    pause
    exit /b 0
)

echo.
echo Klasör kopyalanıyor...

REM Klasörü kopyala
xcopy "%SOURCE_FOLDER%" "%TARGET_FOLDER%" /E /I /H /Y

REM İşlem sonucunu kontrol et
if exist "%TARGET_FOLDER%" (
    echo.
    echo ✓ BAŞARILI: Klasör başarıyla kopyalandı!
    echo Kaynak: %SOURCE_FOLDER%
    echo Hedef:  %TARGET_FOLDER%
    
    echo.
    echo Kaynak klasör siliniyor...
    rmdir /s /q "%SOURCE_FOLDER%"
    
    if not exist "%SOURCE_FOLDER%" (
        echo ✓ Kaynak klasör başarıyla silindi!
    ) else (
        echo ⚠ UYARI: Kaynak klasör silinemedi!
        echo Lütfen manuel olarak silin: %SOURCE_FOLDER%
    )
) else (
    echo.
    echo ✗ HATA: Klasör kopyalanamadı!
    echo Lütfen yetkilerinizi kontrol edin.
)

echo.
pause
