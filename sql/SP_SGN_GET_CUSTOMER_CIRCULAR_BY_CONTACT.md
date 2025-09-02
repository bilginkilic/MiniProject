# SP_SGN_GET_CUSTOMER_CIRCULAR_BY_CONTACT

## Açıklama
Müşteri numarası ve opsiyonel olarak yetkili kontak numarası ile müşterinin sirküler ve yetkili bilgilerini getirir.

## Parametreler
- `@MusteriNo` (varchar(20)): Müşteri numarası
- `@YetkiliKontaktNo` (varchar(20), opsiyonel): Yetkili kontak numarası. NULL geçilirse müşterinin tüm yetkilileri listelenir.

## Dönüş Değerleri
Aşağıdaki kolonları içeren bir result set döner:
- `MusteriNo`: Müşteri numarası
- `SirkulerNo`: Sirküler numarası
- `SirkulerTarihi`: Sirküler tarihi
- `SirkulerDurumu`: Sirküler durumu
- `YetkiliKontaktNo`: Yetkili kontak numarası
- `YetkiliAdi`: Yetkili adı soyadı
- `YetkiSekli`: Yetki şekli (Müştereken, vb.)
- `YetkiTarihi`: Yetki başlangıç tarihi
- `YetkiBitisTarihi`: Yetki bitiş tarihi
- `YetkiGrubu`: Yetki grubu (A, B, C, vb.)
- `SinirliYetkiDetaylari`: Sınırlı yetki detayları
- `YetkiTurleri`: Yetki türleri
- `YetkiTutari`: Yetki tutarı
- `YetkiDovizCinsi`: Yetki döviz cinsi
- `YetkiDurumu`: Yetki durumu (Aktif/Pasif)
- `GecerlilikDurumu`: Geçerlilik durumu ("Geçerli"/"Süresi Geçmiş")

## Örnek Kullanım (SQL)
```sql
-- Müşterinin tüm yetkilileri
EXEC SP_SGN_GET_CUSTOMER_CIRCULAR_BY_CONTACT @MusteriNo = '12345'

-- Belirli bir yetkili
EXEC SP_SGN_GET_CUSTOMER_CIRCULAR_BY_CONTACT @MusteriNo = '12345', @YetkiliKontaktNo = '5000711'
```

## C# Sınıfları
```csharp
public class CircularYetkiliResult
{
    public string MusteriNo { get; set; }
    public string SirkulerNo { get; set; }
    public string SirkulerTarihi { get; set; }
    public string SirkulerDurumu { get; set; }
    public string YetkiliKontaktNo { get; set; }
    public string YetkiliAdi { get; set; }
    public string YetkiSekli { get; set; }
    public string YetkiTarihi { get; set; }
    public string YetkiBitisTarihi { get; set; }
    public string YetkiGrubu { get; set; }
    public string SinirliYetkiDetaylari { get; set; }
    public string YetkiTurleri { get; set; }
    public decimal YetkiTutari { get; set; }
    public string YetkiDovizCinsi { get; set; }
    public string YetkiDurumu { get; set; }
    public string GecerlilikDurumu { get; set; }
}

public class CircularYetkiliResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public List<CircularYetkiliResult> Data { get; set; }
}

public static CircularYetkiliResponse GetCustomerCircularAndYetkili(string musteriNo, string yetkiliKontaktNo = null)
{
    try
    {
        using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
        {
            conn.Open();
            using (var cmd = new SqlCommand("SP_SGN_GET_CUSTOMER_CIRCULAR_BY_CONTACT", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@MusteriNo", musteriNo);
                cmd.Parameters.AddWithValue("@YetkiliKontaktNo", (object)yetkiliKontaktNo ?? DBNull.Value);

                var results = new List<CircularYetkiliResult>();
                using (var reader = cmd.ExecuteReader())
                {
                    // Eğer Message kolonu dönerse, kayıt yok demektir
                    if (reader.HasRows && reader.GetName(0) == "Message")
                    {
                        reader.Read();
                        return new CircularYetkiliResponse
                        {
                            Success = true,
                            Message = reader.GetString(0),
                            Data = new List<CircularYetkiliResult>()
                        };
                    }

                    // Normal sonuçları oku
                    while (reader.Read())
                    {
                        results.Add(new CircularYetkiliResult
                        {
                            MusteriNo = reader["MusteriNo"].ToString(),
                            SirkulerNo = reader["SirkulerNo"].ToString(),
                            SirkulerTarihi = reader["SirkulerTarihi"].ToString(),
                            SirkulerDurumu = reader["SirkulerDurumu"].ToString(),
                            YetkiliKontaktNo = reader["YetkiliKontaktNo"].ToString(),
                            YetkiliAdi = reader["YetkiliAdi"].ToString(),
                            YetkiSekli = reader["YetkiSekli"].ToString(),
                            YetkiTarihi = reader["YetkiTarihi"].ToString(),
                            YetkiBitisTarihi = reader["YetkiBitisTarihi"].ToString(),
                            YetkiGrubu = reader["YetkiGrubu"].ToString(),
                            SinirliYetkiDetaylari = reader["SinirliYetkiDetaylari"].ToString(),
                            YetkiTurleri = reader["YetkiTurleri"].ToString(),
                            YetkiTutari = Convert.ToDecimal(reader["YetkiTutari"]),
                            YetkiDovizCinsi = reader["YetkiDovizCinsi"].ToString(),
                            YetkiDurumu = reader["YetkiDurumu"].ToString(),
                            GecerlilikDurumu = reader["GecerlilikDurumu"].ToString()
                        });
                    }
                }

                return new CircularYetkiliResponse
                {
                    Success = true,
                    Message = results.Any() ? "Kayıtlar başarıyla getirildi." : "Kayıt bulunamadı.",
                    Data = results
                };
            }
        }
    }
    catch (Exception ex)
    {
        return new CircularYetkiliResponse
        {
            Success = false,
            Message = $"Hata oluştu: {ex.Message}",
            Data = new List<CircularYetkiliResult>()
        };
    }
}
```

## Örnek Kullanım (C#)
```csharp
// Müşterinin tüm yetkilileri
var response1 = GetCustomerCircularAndYetkili("12345");

// Belirli bir yetkili
var response2 = GetCustomerCircularAndYetkili("12345", "5000711");

if (response2.Success)
{
    if (response2.Data.Any())
    {
        foreach (var yetkili in response2.Data)
        {
            Console.WriteLine($"Sirküler No: {yetkili.SirkulerNo}");
            Console.WriteLine($"Yetkili: {yetkili.YetkiliAdi}");
            Console.WriteLine($"Geçerlilik: {yetkili.GecerlilikDurumu}");
            
            if (yetkili.GecerlilikDurumu == "Süresi Geçmiş")
            {
                Console.WriteLine("Bu yetkilinin yetki süresi dolmuş!");
            }
        }
    }
    else
    {
        Console.WriteLine(response2.Message); // "Kayıt bulunamadı" veya diğer bilgi mesajları
    }
}
else
{
    Console.WriteLine($"Hata: {response2.Message}");
}
```

## Notlar
- Stored procedure önce müşterinin aktif sirküleri olup olmadığını kontrol eder
- Sirküler varsa, yetkilileri listeler
- Yetkili kontak no verilmişse sadece o yetkiliyi filtreler
- Yetki süresi kontrolü otomatik yapılır
- Sadece aktif sirküler ve yetkilileri listeler
- Sonuçlar sirküler ve yetki tarihine göre sıralıdır