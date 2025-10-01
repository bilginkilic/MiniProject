# Python Paketleri Offline Kurulum Kılavuzu (32-bit)

Bu klasör, projenin çalışması için gerekli Python paketlerinin Windows 32-bit (x86) için offline kurulum dosyalarını içerir.

## İndirilen Paketler

Ana Paketler:
- pandas (2.0.3)
- numpy (1.26.4)
- matplotlib (3.7.5)
- pywin32 (311)
- python-dateutil (2.9.0.post0)

Bağımlılıklar:
- contourpy (1.3.3)
- cycler (0.12.1)
- fonttools (4.60.1)
- kiwisolver (1.4.7)
- packaging (25.0)
- pillow (11.3.0)
- pyparsing (3.2.5)
- pytz (2025.2)
- six (1.17.0)
- tzdata (2025.2)

## Kurulum Adımları (Windows 32-bit)

1. Bu klasörü offline Windows bilgisayara kopyalayın
2. Command Prompt'u yönetici olarak açın ve bu klasöre gidin:
   ```cmd
   cd path\to\python_packages_x86
   ```
3. Aşağıdaki komutu çalıştırın:
   ```cmd
   python -m pip install --no-index --find-links=. pandas numpy matplotlib pywin32 python-dateutil
   ```

## Önemli Notlar

1. Bu paketler Python 3.11 sürümü için Windows 32-bit (x86) platformunda indirilmiştir.
2. 32-bit Python kurulumunuz olduğundan emin olun.

3. Aşağıdaki modüller Python'un standart kütüphanesinin parçasıdır ve ayrıca kurulum gerektirmez:
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
   
2. 32-bit Python kullandığınızdan emin olun:
   ```cmd
   python -c "import platform; print(platform.architecture()[0])"
   ```
   Çıktı "32bit" olmalıdır.

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
- Platform: Windows 32-bit (x86)
- Tarih: Ekim 2025

## Not

Eğer 64-bit Windows kullanıyorsanız, lütfen `python_packages` klasöründeki 64-bit sürümleri kullanın. Bu klasördeki paketler sadece 32-bit Windows sistemleri içindir.
