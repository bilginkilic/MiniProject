# ServisContex - SMTP Mail Service with Retry Logic

Bu Windows servisi, SMTP bağlantılarını yönetir ve hata durumlarında otomatik olarak yeniden deneme mekanizması içerir. Gündüz ve gece saatleri için farklı tolerans süreleri kullanır.

## Özellikler

- Her dakika SMTP bağlantısını kontrol eder
- **Gündüz saatleri** (08:00-18:00): 5 dakika boyunca yeniden deneme
- **Gece saatleri** (18:00-08:00): 45 dakika boyunca yeniden deneme
- Her deneme arasında 60 saniye bekler
- Maksimum deneme süresi sonunda hala başarısız olursa servisi durdurur
- Task Scheduler ile otomatik yeniden başlatma için hazır

## Kurulum

### 1. Projeyi Derleyin
```bash
# Visual Studio'da projeyi açın ve Release modunda derleyin
# veya komut satırından:
msbuild ServisContex.csproj /p:Configuration=Release
```

### 2. SMTP Ayarlarını Yapılandırın
`App.config` dosyasındaki SMTP ayarlarını düzenleyin:

```xml
<appSettings>
    <add key="SmtpServer" value="smtp.gmail.com" />
    <add key="SmtpPort" value="587" />
    <add key="SmtpUsername" value="your-email@gmail.com" />
    <add key="SmtpPassword" value="your-app-password" />
    <add key="EnableSsl" value="true" />
</appSettings>
```

### 3. Gündüz/Gece Tolerans Ayarlarını Yapılandırın
`App.config` dosyasındaki tolerans ayarlarını düzenleyin:

```xml
<!-- Gündüz/Gece Tolerans Ayarları -->
<add key="DayMaxRetryAttempts" value="5" />      <!-- Gündüz maksimum deneme sayısı -->
<add key="NightMaxRetryAttempts" value="45" />   <!-- Gece maksimum deneme sayısı -->
<add key="DayStartHour" value="8" />             <!-- Gündüz başlangıç saati -->
<add key="DayEndHour" value="18" />              <!-- Gündüz bitiş saati -->
```

### 4. Servisi Yükleyin
Yönetici olarak komut satırını açın ve şu komutu çalıştırın:

```bash
# Servisi yükle
installutil.exe "C:\path\to\ServisContex.exe"

# veya program içinden:
ServisContex.exe install
```

### 5. Servisi Başlatın
```bash
# Servisi başlat
net start ServisContex

# veya program içinden:
ServisContex.exe start
```

## Kullanım

### Komut Satırı Parametreleri

```bash
ServisContex.exe install    # Servisi yükle
ServisContex.exe uninstall  # Servisi kaldır
ServisContex.exe start      # Servisi başlat
ServisContex.exe stop       # Servisi durdur
```

### Task Scheduler ile Otomatik Yeniden Başlatma

1. Task Scheduler'ı açın
2. "Create Basic Task" seçin
3. Trigger olarak "When a specific event is logged" seçin
4. Event ID: 7034 (Service stopped)
5. Action olarak servisi başlatma komutu ekleyin:
   ```
   Program: net
   Arguments: start ServisContex
   ```

## Yapılandırma

`App.config` dosyasında aşağıdaki ayarları değiştirebilirsiniz:

### Temel Ayarlar
- `RetryIntervalMinutes`: Her kaç dakikada bir kontrol edileceği (varsayılan: 1)
- `RetryDelaySeconds`: Denemeler arası bekleme süresi (varsayılan: 60)

### Gündüz/Gece Tolerans Ayarları
- `DayMaxRetryAttempts`: Gündüz maksimum deneme sayısı (varsayılan: 5)
- `NightMaxRetryAttempts`: Gece maksimum deneme sayısı (varsayılan: 45)
- `DayStartHour`: Gündüz başlangıç saati (varsayılan: 8)
- `DayEndHour`: Gündüz bitiş saati (varsayılan: 18)

### Örnek Yapılandırmalar

**Ofis Saatleri (09:00-17:00):**
```xml
<add key="DayMaxRetryAttempts" value="3" />
<add key="NightMaxRetryAttempts" value="60" />
<add key="DayStartHour" value="9" />
<add key="DayEndHour" value="17" />
```

**7/24 Yüksek Tolerans:**
```xml
<add key="DayMaxRetryAttempts" value="10" />
<add key="NightMaxRetryAttempts" value="120" />
<add key="DayStartHour" value="6" />
<add key="DayEndHour" value="22" />
```

## Loglama

Servis, tüm işlemleri konsola ve Windows Event Log'a kaydeder. Logları görmek için:

1. Event Viewer'ı açın
2. Windows Logs > Application bölümüne gidin
3. "ServisContex" kaynaklı logları arayın

### Örnek Log Çıktıları

```
[2024-01-15 08:30:00] Servis başarıyla başlatıldı. Timer aktif.
[2024-01-15 08:30:00] Gündüz toleransı: 5 dakika (8:00-18:00)
[2024-01-15 08:30:00] Gece toleransı: 45 dakika (18:00-8:00)
[2024-01-15 08:31:00] SMTP bağlantısı deneniyor...
[2024-01-15 08:31:00] Gündüz modu aktif - Tolerans: 5 dakika
[2024-01-15 08:31:00] SMTP bağlantı denemesi 1/5
```

## Sorun Giderme

### Servis Başlatılamıyor
- Yönetici hakları ile çalıştırdığınızdan emin olun
- SMTP ayarlarının doğru olduğunu kontrol edin
- Firewall ayarlarını kontrol edin

### SMTP Bağlantı Hatası
- SMTP sunucu adresini ve portunu kontrol edin
- Kullanıcı adı ve şifrenin doğru olduğunu kontrol edin
- SSL ayarlarını kontrol edin

### Servis Sürekli Yeniden Başlıyor
- Task Scheduler ayarlarını kontrol edin
- SMTP sunucusunun erişilebilir olduğunu kontrol edin
- Tolerans ayarlarının uygun olduğunu kontrol edin

## Geliştirme

Projeyi geliştirmek için:

1. Visual Studio'da projeyi açın
2. `WindowsService.cs` dosyasını düzenleyin
3. Debug modunda test edin
4. Release modunda derleyin

## Lisans

Bu proje MIT lisansı altında lisanslanmıştır. 