# SP_SGN_CIRCULAR_GET_ALL_ACTIVE

Bu stored procedure, sirküler listesi için grid kolonlarını hazırlar.

## Grid Kolonları

| Kolon Adı | Alan Adı | Genişlik | Özellik |
|-----------|----------|----------|----------|
| Detail | - | 50px | Buton kolonu |
| Sirküler Ref. | CircularRef | 100px | |
| Müşteri No | CustomerNo | 100px | |
| Firma Ünvanı | CompanyTitle | 200px | |
| Düzenleme Tarihi | IssuedDate | 120px | Tarih formatı |
| Geçerlilik Tarihi | ValidityDate | 120px | Tarih formatı |
| Sirküler Tipi | CircularType | 150px | |
| Sirküler Noter No | CircularNotaryNo | 120px | |
| Özel Durumlar | SpecialCases | 200px | |
| Sirküler Durumu | CircularStatus | 120px | |
| Sirküler Belge | - | 100px | "Görüntüle" linki |

## Arama Kriterleri

| Kriter | Açıklama |
|--------|-----------|
| Sirküler Durumu | Dropdown liste |
| Müşteri No | Text arama |
| Sirküler Ref. | Text arama |
| Yetki Türleri | Dropdown (örn: "Kredi İşlemleri, Hazine İşlemleri") |
| Yetki Şekli | Dropdown (örn: "Münferiden, Sınırsız Müşterek") |
| Düzenleme Tarihi | Tarih aralığı seçimi |
| Geçerlilik Tarihi | Tarih aralığı seçimi |
| Sirküler Tipi | Dropdown liste |
| Yetkili Kontakt No | Text arama (örn: 5000711) |

## Grid Yapılandırması

```csharp
dataGridView1.Columns.Clear();
dataGridView1.AutoGenerateColumns = false;

// Detail butonu
var btnDetail = Helper.StaticHelper.GetOneGridButton("Detail", "ID", 50, "Detail");
dataGridView1.Columns.Add(btnDetail);

// Sirküler Ref.
var col = Helper.StaticHelper.GetOneGridColumn("Sirküler Ref.", "CircularRef", 100);
dataGridView1.Columns.Add(col);

// Müşteri No
col = Helper.StaticHelper.GetOneGridColumn("Müşteri No", "CustomerNo", 100);
dataGridView1.Columns.Add(col);

// Firma Ünvanı
col = Helper.StaticHelper.GetOneGridColumn("Firma Ünvanı", "CompanyTitle", 200);
dataGridView1.Columns.Add(col);

// Düzenleme Tarihi
col = Helper.StaticHelper.GetOneGridColumn("Düzenleme Tarihi", "IssuedDate", 120);
col.DefaultCellStyle.Format = "dd/MM/yyyy";
dataGridView1.Columns.Add(col);

// Geçerlilik Tarihi
col = Helper.StaticHelper.GetOneGridColumn("Geçerlilik Tarihi", "ValidityDate", 120);
col.DefaultCellStyle.Format = "dd/MM/yyyy";
dataGridView1.Columns.Add(col);

// Sirküler Tipi
col = Helper.StaticHelper.GetOneGridColumn("Sirküler Tipi", "CircularType", 150);
dataGridView1.Columns.Add(col);

// Sirküler Noter No
col = Helper.StaticHelper.GetOneGridColumn("Sirküler Noter No", "CircularNotaryNo", 120);
dataGridView1.Columns.Add(col);

// Özel Durumlar
col = Helper.StaticHelper.GetOneGridColumn("Özel Durumlar", "SpecialCases", 200);
dataGridView1.Columns.Add(col);

// Sirküler Durumu
col = Helper.StaticHelper.GetOneGridColumn("Sirküler Durumu", "CircularStatus", 120);
dataGridView1.Columns.Add(col);

// Sirküler Belge (Görüntüle linki)
var btnView = Helper.StaticHelper.GetOneGridButton("Sirküler Belge", "ID", 100, "Görüntüle");
dataGridView1.Columns.Add(btnView);
```

## Özellikler

1. Tüm kolonlar için Türkçe başlıklar
2. Tarih kolonları için format ayarı (dd/MM/yyyy)
3. Detail ve Görüntüle için özel buton kolonları
4. Her kolon için özel genişlik tanımları
5. Arama kriterleri için filtre desteği
6. Sadece aktif kayıtları listeler (RecordStatus = 'A')
7. CreateDate'e göre DESC sıralama

## Örnek Kullanım

```sql
EXEC [dbo].[SP_SGN_CIRCULAR_GET_ALL_ACTIVE]
```

## DAL Katmanı Kullanımı

```csharp
// Tüm aktif sirkülerleri getir
var circulars = SignatureAuthDAL.GetAllActiveCirculars();

// Grid'e bağla
dataGridView1.DataSource = circulars;
```