# Python Paketleri Offline Kurulum Kılavuzu

Bu klasör, projenin çalışması için gerekli Python paketlerinin offline kurulum dosyalarını içerir.

## İndirilen Paketler

Ana Paketler:
- pandas (2.3.3)
- numpy (2.3.3)
- matplotlib (3.10.6)
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

## Kurulum Adımları

1. Bu klasörü offline bilgisayara kopyalayın
2. Terminal veya Command Prompt'ta bu klasöre gidin:
   ```bash
   cd path/to/python_packages
   ```
3. Aşağıdaki komutu çalıştırın:
   ```bash
   python3 -m pip install --no-index --find-links=. pandas numpy matplotlib python-dateutil
   ```

## Önemli Notlar

1. Bu paketler Python 3.11.4 sürümü için MacOS (arm64) platformunda indirilmiştir.

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

3. Windows kullanıcıları için:
   - Bu paketler MacOS için indirilmiştir
   - Windows sisteminde kullanmak için, internet bağlantısı olan bir Windows bilgisayarda aşağıdaki komutu çalıştırarak Windows uyumlu paketleri indirmeniz gerekir:
     ```bash
     pip download -d ./python_packages pandas numpy matplotlib pywin32 python-dateutil --only-binary=:all:
     ```

## Sorun Giderme

Eğer kurulum sırasında hata alırsanız:

1. Python sürümünüzü kontrol edin:
   ```bash
   python3 --version
   ```
   
2. İşletim sisteminize uygun paketleri indirdiğinizden emin olun

3. Pip'in güncel olduğundan emin olun:
   ```bash
   python3 -m pip install --upgrade pip
   ```

4. Hala sorun yaşıyorsanız, her paketi tek tek kurmayı deneyin:
   ```bash
   python3 -m pip install --no-index --find-links=. paket_adi
   ```

## Versiyon Bilgileri

- Python: 3.11.4
- Platform: MacOS (arm64)
- Tarih: Ekim 2025
