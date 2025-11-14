# PivotJS Excel Export Örnekleri

Bu klasör, PivotJS (PivotTable.js) ile oluşturulan pivot tabloları Excel formatına aktarmak için örnek kodlar içerir.

## Dosyalar

- **index.html**: Tam çalışan örnek HTML sayfası
- **export-helper.js**: Yeniden kullanılabilir Excel export fonksiyonları
- **README.md**: Bu dosya

## Kullanım

### 1. Basit Kullanım (index.html)

`index.html` dosyasını bir web tarayıcısında açın. Sayfa şunları içerir:

- Örnek veri ile oluşturulmuş bir pivot tablo
- "Excel'e Aktar" butonu (pivot tabloyu Excel'e aktarır)
- "Ham Veriyi Excel'e Aktar" butonu (orijinal veriyi Excel'e aktarır)

### 2. Gelişmiş Kullanım (export-helper.js)

`export-helper.js` dosyasını projenize dahil edin ve fonksiyonları kullanın:

```html
<!-- Gerekli kütüphaneleri yükleyin -->
<script src="https://cdnjs.cloudflare.com/ajax/libs/jquery/3.6.0/jquery.min.js"></script>
<script src="https://cdn.jsdelivr.net/npm/pivottable@2.0.0/dist/pivot.min.js"></script>
<script src="https://cdnjs.cloudflare.com/ajax/libs/xlsx/0.18.5/xlsx.full.min.js"></script>

<!-- Helper fonksiyonları dahil edin -->
<script src="export-helper.js"></script>
```

#### Örnek 1: Pivot Tabloyu Excel'e Aktar

```javascript
// Pivot tabloyu oluştur
$("#output").pivotUI(data, {
    rows: ["Bölge"],
    cols: ["Ay"],
    vals: ["Satış"],
    aggregatorName: "Sum"
});

// Excel'e aktar
function exportToExcel() {
    try {
        exportPivotTableToExcel("#output table.pvtTable", "Rapor.xlsx", "Satış Raporu");
        console.log("Excel dosyası başarıyla oluşturuldu!");
    } catch (error) {
        console.error("Hata:", error.message);
    }
}
```

#### Örnek 2: JSON Verisini Excel'e Aktar

```javascript
const data = [
    { Bölge: "İstanbul", Ürün: "Laptop", Satış: 150 },
    { Bölge: "Ankara", Ürün: "Mouse", Satış: 50 }
];

// Excel'e aktar
exportJsonToExcel(data, "Veri.xlsx", "Satışlar", [
    15, // Bölge sütunu genişliği
    15, // Ürün sütunu genişliği
    12  // Satış sütunu genişliği
]);
```

#### Örnek 3: Birden Fazla Pivot Tabloyu Tek Dosyaya Aktar

```javascript
const tables = [
    { selector: "#pivot1 table.pvtTable", sheetName: "Satış Raporu" },
    { selector: "#pivot2 table.pvtTable", sheetName: "Ürün Analizi" },
    { selector: "#pivot3 table.pvtTable", sheetName: "Bölge Karşılaştırması" }
];

exportMultiplePivotTablesToExcel(tables, "Kapsamlı_Rapor.xlsx");
```

#### Örnek 4: PivotData Objesini Excel'e Aktar

```javascript
// Pivot tabloyu oluştur ve pivotData'yı al
let pivotData = null;

$("#output").pivotUI(data, {
    rows: ["Bölge"],
    cols: ["Ay"],
    vals: ["Satış"],
    aggregatorName: "Sum",
    onRefresh: function(config) {
        pivotData = config;
    }
});

// PivotData'yı Excel'e aktar
function exportPivotData() {
    if (pivotData) {
        exportPivotDataToExcel(pivotData, "Pivot_Veri.xlsx", "Analiz");
    }
}
```

## Gereksinimler

### CDN Linkleri

```html
<!-- jQuery (PivotTable.js için gerekli) -->
<script src="https://cdnjs.cloudflare.com/ajax/libs/jquery/3.6.0/jquery.min.js"></script>

<!-- PivotTable.js -->
<script src="https://cdn.jsdelivr.net/npm/pivottable@2.0.0/dist/pivot.min.js"></script>
<link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/pivottable@2.0.0/dist/pivot.css">

<!-- SheetJS (Excel export için) -->
<script src="https://cdnjs.cloudflare.com/ajax/libs/xlsx/0.18.5/xlsx.full.min.js"></script>
```

### NPM ile Kurulum (Alternatif)

```bash
npm install pivottable xlsx
```

## Fonksiyonlar

### exportPivotTableToExcel(tableSelector, fileName, sheetName)

Pivot tabloyu Excel formatına aktarır.

**Parametreler:**
- `tableSelector` (string): Pivot tablonun CSS selector'ı
- `fileName` (string, opsiyonel): İndirilecek dosya adı
- `sheetName` (string, opsiyonel): Excel çalışma sayfası adı

**Örnek:**
```javascript
exportPivotTableToExcel("#output table.pvtTable", "Rapor.xlsx", "Satış Raporu");
```

### exportJsonToExcel(data, fileName, sheetName, colWidths)

JSON verisini Excel formatına aktarır.

**Parametreler:**
- `data` (Array): Excel'e aktarılacak veri array'i
- `fileName` (string, opsiyonel): İndirilecek dosya adı
- `sheetName` (string, opsiyonel): Excel çalışma sayfası adı
- `colWidths` (Array, opsiyonel): Sütun genişlikleri

**Örnek:**
```javascript
exportJsonToExcel(data, "Veri.xlsx", "Satışlar", [15, 15, 12]);
```

### exportMultiplePivotTablesToExcel(tables, fileName)

Birden fazla pivot tabloyu tek bir Excel dosyasına aktarır.

**Parametreler:**
- `tables` (Array): Tablo bilgileri array'i `[{selector, sheetName}, ...]`
- `fileName` (string, opsiyonel): İndirilecek dosya adı

**Örnek:**
```javascript
exportMultiplePivotTablesToExcel([
    { selector: "#pivot1 table.pvtTable", sheetName: "Rapor 1" },
    { selector: "#pivot2 table.pvtTable", sheetName: "Rapor 2" }
], "Raporlar.xlsx");
```

### exportPivotDataToExcel(pivotData, fileName, sheetName)

PivotJS pivotData objesini Excel formatına aktarır.

**Parametreler:**
- `pivotData` (Object): PivotJS pivotData objesi
- `fileName` (string, opsiyonel): İndirilecek dosya adı
- `sheetName` (string, opsiyonel): Excel çalışma sayfası adı

**Örnek:**
```javascript
exportPivotDataToExcel(pivotData, "Pivot_Veri.xlsx", "Analiz");
```

## Özellikler

- ✅ Pivot tabloları Excel formatına aktarma
- ✅ Ham veriyi Excel formatına aktarma
- ✅ Birden fazla tabloyu tek dosyaya aktarma
- ✅ Otomatik sütun genişliği ayarlama
- ✅ Özel sütun genişlikleri belirleme
- ✅ Türkçe karakter desteği
- ✅ Hata yönetimi

## Tarayıcı Desteği

- Chrome/Edge (önerilen)
- Firefox
- Safari
- Opera

## Notlar

1. SheetJS kütüphanesi Excel dosyalarını oluşturmak için kullanılır
2. Dosyalar tarayıcının varsayılan indirme klasörüne kaydedilir
3. Büyük veri setleri için performans optimizasyonu gerekebilir
4. Excel dosyaları `.xlsx` formatında oluşturulur

## Sorun Giderme

### "SheetJS (XLSX) kütüphanesi yüklenmemiş!" hatası

Çözüm: SheetJS kütüphanesini sayfanıza dahil edin:
```html
<script src="https://cdnjs.cloudflare.com/ajax/libs/xlsx/0.18.5/xlsx.full.min.js"></script>
```

### "Pivot tablo bulunamadı!" hatası

Çözüm: CSS selector'ın doğru olduğundan emin olun. Pivot tablo oluşturulduktan sonra export işlemini yapın.

### Türkçe karakterler bozuk görünüyor

Çözüm: HTML sayfanızın charset'ini UTF-8 olarak ayarlayın:
```html
<meta charset="UTF-8">
```

## Lisans

Bu örnek kodlar eğitim amaçlıdır ve serbestçe kullanılabilir.

