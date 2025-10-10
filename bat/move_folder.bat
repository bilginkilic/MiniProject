@echo off
REM Klasör Kopyalama Scripti
REM Kullanım: move_folder.bat "kaynak_klasör" "hedef_klasör"

setlocal enabledelayedexpansion

REM Parametreleri kontrol et
if "%~1"=="" (
    echo HATA: Kaynak klasör parametresi eksik!
    echo Kullanım: %0 "kaynak_klasör" "hedef_klasör"
    echo Örnek: %0 "C:\temp\eski" "C:\temp\yeni"
    pause
    exit /b 1
)

if "%~2"=="" (
    echo HATA: Hedef klasör parametresi eksik!
    echo Kullanım: %0 "kaynak_klasör" "hedef_klasör"
    echo Örnek: %0 "C:\temp\eski" "C:\temp\yeni"
    pause
    exit /b 1
)

set "SOURCE_FOLDER=%~1"
set "TARGET_FOLDER=%~2"

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
echo KLASÖR KOPYALAMA İŞLEMİ
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
    echo Kaynak: %SOURCE_FOLDER% (orijinal korundu)
    echo Hedef:  %TARGET_FOLDER%
) else (
    echo.
    echo ✗ HATA: Klasör kopyalanamadı!
    echo Lütfen yetkilerinizi kontrol edin.
)

echo.
pause
