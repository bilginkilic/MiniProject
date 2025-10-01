# Python Paketleri Offline Kurulum Kılavuzu

Bu klasör, projenin çalışması için gerekli Python paketlerinin Windows için offline kurulum dosyalarını içerir.

## İndirilen Paketler

Ana Paketler:
- pandas (2.3.3)
- numpy (2.3.3)
- matplotlib (3.10.6)
- pywin32 (311)
- python-dateutil (2.9.0.post0)

Bağımlılıklar:
- contourpy (1.3.3)
- cycler (0.12.1)
- fonttools (4.60.1)
- kiwisolver (1.4.9)
- packaging (25.0)
- pillow (11.3.0)
- pyparsing (3.2.5)
- pytz (2025.2)
- six (1.17.0)
- tzdata (2025.2)

## Kurulum Adımları (Windows)

1. Bu klasörü offline Windows bilgisayara kopyalayın
2. Command Prompt'u yönetici olarak açın ve bu klasöre gidin:
   ```cmd
   cd path\to\python_packages
   ```
3. Aşağıdaki komutu çalıştırın:
   ```cmd
   python -m pip install --no-index --find-links=. pandas numpy matplotlib pywin32 python-dateutil
   ```

## Önemli Notlar

1. Bu paketler Python 3.11 sürümü için Windows (64-bit) platformunda indirilmiştir.

2. Aşağıdaki modüller Python'un standart kütüphanesinin parçasıdır ve ayrıca kurulum gerektirmez:
   - time
   - datetime
   - warnings
   - os
   - math
   - calendar
   - typing
   - pickle
   - csv

## Sorun Giderme

Eğer kurulum sırasında hata alırsanız:

1. Python sürümünüzü kontrol edin:
   ```cmd
   python --version
   ```
   
2. 64-bit Python kullandığınızdan emin olun:
   ```cmd
   python -c "import platform; print(platform.architecture()[0])"
   ```

3. Pip'in güncel olduğundan emin olun:
   ```cmd
   python -m pip install --upgrade pip
   ```

4. Hala sorun yaşıyorsanız, her paketi tek tek kurmayı deneyin:
   ```cmd
   python -m pip install --no-index --find-links=. paket_adi
   ```

## Versiyon Bilgileri

- Python: 3.11
- Platform: Windows (64-bit)
- Tarih: Ekim 2025