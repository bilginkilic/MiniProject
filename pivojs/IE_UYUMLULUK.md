# IE Uyumluluk ve Sunucu Tabanlı Excel Export

## Sorun

Internet Explorer (IE) web browser'da `slice` object does not support hatası alınıyordu. Bu sorun, JavaScript'te `slice()` metodunun IE'nin eski versiyonlarında desteklenmemesinden kaynaklanıyordu.

## Çözüm

Sunucu tabanlı Excel export çözümü uygulandı. Artık veriler sunucuya gönderiliyor ve Aspose.Cells kütüphanesi ile Excel dosyası oluşturuluyor.

## Değişiklikler

### 1. ExcelExport.ashx (Yeni Dosya)
- Sunucu tarafında Excel export handler'ı
- Aspose.Cells kullanarak Excel dosyası oluşturuyor
- IE uyumlu response header'ları kullanıyor
- POST ve GET metodlarını destekliyor (IE uyumluluğu için)

**Konum:** `possibilities/ExcelExport.ashx`

### 2. export-helper.js (Güncellendi)
- `slice()` yerine `substring()` kullanılıyor (IE uyumlu)
- Sunucu tabanlı export fonksiyonları eklendi
- IE uyumlu XMLHttpRequest desteği
- IE için özel dosya indirme yöntemleri (msSaveOrOpenBlob, iframe fallback)

**Değişiklikler:**
- `formatDateForFileName()` - IE uyumlu tarih formatı
- `createXHR()` - IE uyumlu XMLHttpRequest
- `exportToExcelServer()` - Sunucu tabanlı export
- Tüm export fonksiyonları varsayılan olarak sunucu tabanlı çalışıyor

### 3. index.html (Güncellendi)
- `export-helper.js` dahil edildi
- SheetJS (XLSX) kütüphanesi artık gerekli değil (yorum satırına alındı)
- Export fonksiyonları sunucu tabanlı çalışacak şekilde güncellendi

### 4. Web.config (Güncellendi)
- ExcelExport handler'ı eklendi

## Kullanım

### Temel Kullanım

```javascript
// Pivot tabloyu Excel'e aktar
exportPivotTableToExcel("#output table.pvtTable", "Rapor.xlsx", "Pivot Tablo")
    .then(function(result) {
        console.log("Başarılı:", result.fileName);
    })
    .catch(function(error) {
        console.error("Hata:", error);
    });

// JSON verisini Excel'e aktar
exportJsonToExcel(data, "Veri.xlsx", "Veri")
    .then(function(result) {
        console.log("Başarılı:", result.fileName);
    });
```

### Sunucu URL'i Belirleme

Varsayılan olarak `ExcelExport.ashx` kullanılır. Farklı bir URL kullanmak için:

```javascript
var serverUrl = "path/to/ExcelExport.ashx";
exportPivotTableToExcel(selector, fileName, sheetName, serverUrl, true);
```

### Client-Side Export (Eski Yöntem)

Eğer hala client-side export kullanmak isterseniz (IE uyumlu değil):

```javascript
exportPivotTableToExcel(selector, fileName, sheetName, null, false);
```

## Gereksinimler

### Sunucu Tarafı
- ASP.NET Web Forms
- Aspose.Cells kütüphanesi (NuGet paketi)
- .NET Framework 4.0 veya üzeri

### Client Tarafı
- Modern tarayıcılar veya IE 8+ (sunucu tabanlı export ile)
- jQuery (PivotTable.js için)
- PivotTable.js

## IE Uyumluluk Detayları

### Desteklenen IE Versiyonları
- IE 8+ (sunucu tabanlı export ile)
- IE 10+ (Blob desteği ile daha iyi performans)

### IE'de Çalışma Mantığı
1. **IE 10+**: `msSaveOrOpenBlob` API kullanılır
2. **IE 8-9**: iframe ile dosya indirme yapılır
3. **Modern Tarayıcılar**: Blob ve URL.createObjectURL kullanılır

### XMLHttpRequest
- Modern tarayıcılar: Standart XMLHttpRequest
- IE 6-7: ActiveXObject ile XMLHttpRequest oluşturulur

## Avantajlar

1. ✅ IE uyumluluğu
2. ✅ Aspose.Cells'in güçlü özellikleri
3. ✅ Büyük veri setleri için daha iyi performans
4. ✅ Sunucu tarafında işlem (güvenlik)
5. ✅ Daha az client-side bağımlılık

## Notlar

- SheetJS (XLSX) artık gerekli değil (opsiyonel)
- Tüm export işlemleri varsayılan olarak sunucu tabanlı
- IE uyumluluğu için `slice()` yerine `substring()` kullanılıyor
- Promise tabanlı API (IE için polyfill gerekebilir)

## Promise Polyfill (IE için)

IE 8-10 Promise desteklemiyor. Gerekirse polyfill ekleyin:

```html
<script src="https://cdn.jsdelivr.net/npm/es6-promise@4/dist/es6-promise.auto.min.js"></script>
```

## Sorun Giderme

### "ExcelExport.ashx bulunamadı" hatası
- Handler'ın `possibilities` klasöründe olduğundan emin olun
- Web.config'de handler kaydının olduğunu kontrol edin

### "Aspose.Cells bulunamadı" hatası
- Aspose.Cells NuGet paketini yükleyin
- Referans'ın doğru olduğunu kontrol edin

### IE'de dosya indirilmiyor
- IE 8-9 için iframe fallback kullanılıyor
- Pop-up blocker'ı kontrol edin
- Güvenlik ayarlarını kontrol edin

