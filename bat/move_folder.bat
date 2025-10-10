@echo off
REM Dosya Taşıma Scripti
REM Parametreler dosyada gömülü olarak tanımlanmıştır
REM İşlem: Dosyaları taşı → Log yaz

setlocal enabledelayedexpansion

REM ========================================
REM PARAMETRELER - Burayı ihtiyacınıza göre düzenleyin
REM ========================================
set "SOURCE_FOLDER=C:\temp\eski"
set "TARGET_FOLDER=C:\temp\yeni"
REM ========================================

REM Log dosyası için tarih oluştur
for /f "tokens=2 delims==" %%a in ('wmic OS Get localdatetime /value') do set "dt=%%a"
set "YY=%dt:~2,2%" & set "YYYY=%dt:~0,4%" & set "MM=%dt:~4,2%" & set "DD=%dt:~6,2%"
set "HH=%dt:~8,2%" & set "Min=%dt:~10,2%" & set "Sec=%dt:~12,2%"
set "LOG_FILE=%TARGET_FOLDER%\tasima_log_%YYYY%%MM%%DD%_%HH%%Min%%Sec%.txt"

echo.
echo ========================================
echo DOSYA TASIMA SCRIPTI
echo ========================================
echo Kaynak: %SOURCE_FOLDER%
echo Hedef:  %TARGET_FOLDER%
echo Log:    %LOG_FILE%
echo ========================================
echo.

REM Kaynak klasörün varlığını kontrol et
if not exist "%SOURCE_FOLDER%" (
    echo HATA: Kaynak klasör bulunamadı: %SOURCE_FOLDER%
    echo %date% %time% - HATA: Kaynak klasör bulunamadı: %SOURCE_FOLDER% > "%LOG_FILE%"
    pause
    exit /b 1
)

REM Hedef klasörün parent dizininin varlığını kontrol et
for %%F in ("%TARGET_FOLDER%") do set "TARGET_PARENT=%%~dpF"
if not exist "%TARGET_PARENT%" (
    echo HATA: Hedef klasörün parent dizini bulunamadı: %TARGET_PARENT%
    echo %date% %time% - HATA: Hedef klasörün parent dizini bulunamadı: %TARGET_PARENT% > "%LOG_FILE%"
    pause
    exit /b 1
)

REM Hedef klasörü oluştur (yoksa)
if not exist "%TARGET_FOLDER%" (
    mkdir "%TARGET_FOLDER%"
)

REM Log dosyasını başlat
echo %date% %time% - Dosya tasima islemi basladi > "%LOG_FILE%"
echo Kaynak: %SOURCE_FOLDER% >> "%LOG_FILE%"
echo Hedef: %TARGET_FOLDER% >> "%LOG_FILE%"
echo. >> "%LOG_FILE%"

echo Dosyalar tasiniyor...

REM Dosyaları taşı (.txt dosyaları hariç)
set "SUCCESS_COUNT=0"
set "ERROR_COUNT=0"

for %%F in ("%SOURCE_FOLDER%\*") do (
    if not exist "%%F\" (
        call :ProcessFile "%%F"
    )
)

goto :ShowResults

:ProcessFile
set "filepath=%~1"
set "filename=%~nx1"
set "extension=%~x1"

REM .txt dosyalarını atla
if /i not "%extension%"==".txt" (
    echo Tasiniyor: %filename%
    move "%filepath%" "%TARGET_FOLDER%\" >nul 2>&1
    if !errorlevel! equ 0 (
        set /a "SUCCESS_COUNT+=1"
        echo %date% %time% - BASARILI: %filename% tasindi >> "%LOG_FILE%"
    ) else (
        set /a "ERROR_COUNT+=1"
        echo %date% %time% - HATA: %filename% tasinamadi >> "%LOG_FILE%"
    )
) else (
    echo Atlaniyor (.txt): %filename%
    echo %date% %time% - ATLANDI (.txt): %filename% >> "%LOG_FILE%"
)
goto :eof

:ShowResults

REM Sonuçları göster ve log'a yaz
echo.
echo ========================================
echo ISLEM TAMAMLANDI
echo ========================================
echo Basarili: %SUCCESS_COUNT% dosya
echo Hatali: %ERROR_COUNT% dosya
echo ========================================

echo. >> "%LOG_FILE%"
echo %date% %time% - Islem tamamlandi >> "%LOG_FILE%"
echo Basarili: %SUCCESS_COUNT% dosya >> "%LOG_FILE%"
echo Hatali: %ERROR_COUNT% dosya >> "%LOG_FILE%"

if %ERROR_COUNT% gtr 0 (
    echo.
    echo UYARI: %ERROR_COUNT% dosya tasinamadi!
    echo Detaylar icin log dosyasini kontrol edin: %LOG_FILE%
) else (
    echo.
    echo TUM DOSYALAR BASARIYLA TASINDI!
)

echo.
echo Log dosyasi: %LOG_FILE%
pause