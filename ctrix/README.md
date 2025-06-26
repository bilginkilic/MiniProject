# Ctrix - Citrix PACO Uygulama Güncelleme Aracı

Bu araç, Citrix ortamında PACO.Exe uygulamasını sunucu yeniden başlatmaya gerek kalmadan güncellemek için tasarlanmıştır. Aktif kullanıcıları tespit eder, uyarı gönderir ve graceful shutdown ile uygulamayı güvenli şekilde günceller.

## 🎯 **Ana Özellikler**

- **Aktif Kullanıcı Tespiti**: PACO.Exe kullanan aktif kullanıcıları otomatik tespit eder
- **Kullanıcı Uyarıları**: Kullanıcılara popup mesajı ile güncelleme uyarısı gönderir
- **Graceful Shutdown**: Kullanıcıların uygulamayı kapatmasını bekler (5 dakika)
- **Force Shutdown**: Gerekirse zorla uygulamayı kapatır
- **Otomatik Yedekleme**: Güncelleme öncesi otomatik yedek alır
- **Cache Temizleme**: Citrix cache'ini otomatik temizler
- **Yeniden Yayınlama**: Uygulamayı otomatik yeniden yayınlar

## 🚀 **Hızlı Başlangıç**

### 1. Kurulum

```bash
# Projeyi derle
msbuild Ctrix.csproj /p:Configuration=Release

# Yönetici olarak çalıştır
Ctrix.exe check
```

### 2. Yapılandırma

`App.config` dosyasını düzenleyin:

```xml
<appSettings>
    <!-- Citrix Sunucu Ayarları -->
    <add key="CitrixServerUrl" value="https://citrix-server.company.com" />
    <add key="CitrixAdminUsername" value="admin@company.com" />
    <add key="CitrixAdminPassword" value="your-admin-password" />
    
    <!-- Uygulama Ayarları -->
    <add key="AppPath" value="C:\Applications\PACO.Exe" />
    <add key="GracefulShutdownTimeout" value="300" /> <!-- 5 dakika -->
</appSettings>
```

### 3. Kullanım

```bash
# Aktif kullanıcıları kontrol et
Ctrix.exe check

# Kullanıcılara uyarı gönder
Ctrix.exe notify

# Uygulamayı güncelle
Ctrix.exe update C:\Updates\PACO_v2.1.exe

# Sadece cache temizle
Ctrix.exe cache

# Sadece yeniden yayınla
Ctrix.exe republish
```

## 📋 **Komut Referansı**

### `check` - Aktif Kullanıcıları Kontrol Et
```bash
Ctrix.exe check
```
- PACO.Exe kullanan aktif kullanıcıları listeler
- Güvenli güncelleme için ön kontrol

### `notify` - Kullanıcılara Uyarı Gönder
```bash
Ctrix.exe notify
```
- Aktif kullanıcılara popup mesajı gönderir
- Güncelleme uyarısı yapar

### `update` - Uygulamayı Güncelle
```bash
Ctrix.exe update <yeni_dosya_yolu>
```
- Tam güncelleme işlemi yapar
- Graceful shutdown ile güvenli güncelleme

### `cache` - Cache Temizle
```bash
Ctrix.exe cache
```
- Citrix Delivery Controller cache'ini temizler
- StoreFront cache'ini temizler

### `republish` - Yeniden Yayınla
```bash
Ctrix.exe republish
```
- Uygulamayı yeniden yayınlar
- Kullanıcıların yeni versiyonu görmesini sağlar

## 🔄 **Güncelleme Süreci**

### 1. **Ön Kontrol**
```bash
Ctrix.exe check
```
- Aktif kullanıcı var mı kontrol et
- Güvenli güncelleme zamanı mı değerlendir

### 2. **Kullanıcı Uyarısı**
```bash
Ctrix.exe notify
```
- Kullanıcılara 5 dakika uyarısı gönder
- Çalışmalarını kaydetmelerini iste

### 3. **Güncelleme**
```bash
Ctrix.exe update C:\Updates\PACO_v2.1.exe
```
- Otomatik yedek al
- Yeni dosyayı kopyala
- Cache temizle
- Yeniden yayınla

## ⚙️ **Yapılandırma Seçenekleri**

### Citrix Sunucu Ayarları
- `CitrixServerUrl`: Citrix sunucu adresi
- `CitrixAdminUsername`: Admin kullanıcı adı
- `CitrixAdminPassword`: Admin şifresi

### Uygulama Ayarları
- `AppPath`: PACO.Exe dosya yolu
- `GracefulShutdownTimeout`: Bekleme süresi (saniye)

### PowerShell Ayarları
- `PowerShellExecutionPolicy`: Execution policy
- `CitrixSnapinPath`: Citrix snap-in yolu

## 📊 **Örnek Kullanım Senaryoları**

### Senaryo 1: Acil Güncelleme
```bash
# Hızlı kontrol
Ctrix.exe check

# Eğer kullanıcı yoksa direkt güncelle
Ctrix.exe update C:\Hotfix\PACO_urgent.exe
```

### Senaryo 2: Planlı Güncelleme
```bash
# 1. Önceden uyarı gönder
Ctrix.exe notify

# 2. 5 dakika bekle
# 3. Güncelle
Ctrix.exe update C:\Updates\PACO_v2.1.exe
```

### Senaryo 3: Sadece Cache Temizleme
```bash
# Cache sorunu varsa
Ctrix.exe cache
Ctrix.exe republish
```

## 🔧 **Sorun Giderme**

### PowerShell Hatası
```
PowerShell hatası: Add-PSSnapin Citrix*
```
**Çözüm:**
1. Citrix Studio yüklü olduğundan emin olun
2. PowerShell'i yönetici olarak çalıştırın
3. Execution policy'yi kontrol edin:
   ```powershell
   Set-ExecutionPolicy RemoteSigned
   ```

### Kullanıcı Bulunamıyor
```
Aktif kullanıcıları alma hatası
```
**Çözüm:**
1. Citrix admin haklarınızı kontrol edin
2. Citrix sunucu bağlantısını test edin
3. PowerShell snap-in'lerini kontrol edin

### Dosya Güncelleme Hatası
```
Dosya güncelleme hatası: Access denied
```
**Çözüm:**
1. Yönetici hakları ile çalıştırın
2. Dosya izinlerini kontrol edin
3. Antivirus yazılımını geçici devre dışı bırakın

### Cache Temizleme Hatası
```
Cache temizleme hatası
```
**Çözüm:**
1. Citrix Studio'yu yeniden başlatın
2. IIS'i yeniden başlatın
3. StoreFront cache'ini manuel temizleyin

## 📝 **Log Dosyaları**

Araç tüm işlemleri `CitrixAppManager.log` dosyasına kaydeder:

```
[2024-01-15 14:30:00] Uygulama güncelleme işlemi başlatılıyor...
[2024-01-15 14:30:01] 3 aktif kullanıcı bulundu. Graceful shutdown başlatılıyor...
[2024-01-15 14:30:02] Uyarı gönderildi: user1
[2024-01-15 14:30:03] Uyarı gönderildi: user2
[2024-01-15 14:30:04] Uyarı gönderildi: user3
[2024-01-15 14:35:00] Tüm kullanıcılar uygulamayı kapattı.
[2024-01-15 14:35:01] Uygulama dosyaları güncelleniyor...
[2024-01-15 14:35:02] Yedek oluşturuldu: C:\Applications\PACO.Exe.backup.20240115143502
[2024-01-15 14:35:03] Uygulama güncellendi: C:\Applications\PACO.Exe
[2024-01-15 14:35:04] Citrix cache temizlendi.
[2024-01-15 14:35:05] Uygulama yeniden yayınlandı.
[2024-01-15 14:35:06] Uygulama güncelleme işlemi tamamlandı.
```

## 🔒 **Güvenlik**

- Admin hakları gerektirir
- Şifreleri config dosyasında şifreleyin
- Güvenli ağ bağlantısı kullanın
- Log dosyalarını düzenli temizleyin

## 📞 **Destek**

Sorun yaşadığınızda:
1. Log dosyasını kontrol edin
2. PowerShell komutlarını manuel test edin
3. Citrix Studio'yu kontrol edin
4. Sistem yöneticisi ile iletişime geçin

## 📄 **Lisans**

Bu proje MIT lisansı altında lisanslanmıştır. 