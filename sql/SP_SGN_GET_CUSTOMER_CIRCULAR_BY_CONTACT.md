# SP_SGN_GET_CUSTOMER_CIRCULAR_BY_CONTACT

/* v1 - Müşteri ve yetkili arama stored procedure - 2024.01.17 */

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
public class CircularSearchResult
{
    public CircularData Circular { get; set; }
    public List<YetkiliData> Yetkililer { get; set; }

    public CircularSearchResult()
    {
        Circular = new CircularData();
        Yetkililer = new List<YetkiliData>();
    }
}

public class CircularSearchResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public List<CircularSearchResult> Data { get; set; }

    public CircularSearchResponse()
    {
        Data = new List<CircularSearchResult>();
    }
}

public static class CircularDataManager
{
    public static CircularSearchResponse GetCustomerCircularAndYetkili(string musteriNo, string yetkiliKontaktNo = null)
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

                    var results = new List<CircularSearchResult>();
                    var currentCircular = new CircularSearchResult();

                    using (var reader = cmd.ExecuteReader())
                    {
                        // Eğer Message kolonu dönerse, kayıt yok demektir
                        if (reader.HasRows && reader.GetName(0) == "Message")
                        {
                            reader.Read();
                            return new CircularSearchResponse
                            {
                                Success = true,
                                Message = reader.GetString(0),
                                Data = new List<CircularSearchResult>()
                            };
                        }

                        string lastSirkulerNo = null;
                        
                        // Normal sonuçları oku
                        while (reader.Read())
                        {
                            string currentSirkulerNo = reader["SirkulerNo"].ToString();

                            // Yeni sirküler başladıysa
                            if (lastSirkulerNo != currentSirkulerNo)
                            {
                                if (lastSirkulerNo != null)
                                {
                                    results.Add(currentCircular);
                                    currentCircular = new CircularSearchResult();
                                }

                                // Sirküler bilgilerini doldur
                                currentCircular.Circular = new CircularData
                                {
                                    CustomerNo = reader["MusteriNo"].ToString(),
                                    CircularNotaryNo = currentSirkulerNo,
                                    IssuedDate = Convert.ToDateTime(reader["SirkulerTarihi"]),
                                    CircularStatus = reader["SirkulerDurumu"].ToString(),
                                    RecordStatus = "Aktif"
                                };

                                lastSirkulerNo = currentSirkulerNo;
                            }

                            // Yetkili bilgilerini ekle
                            var yetkili = new YetkiliData
                            {
                                YetkiliKontakt = reader["YetkiliKontaktNo"].ToString(),
                                YetkiliAdi = reader["YetkiliAdi"].ToString(),
                                YetkiSekli = reader["YetkiSekli"].ToString(),
                                YetkiTarihi = reader["YetkiTarihi"].ToString(),
                                YetkiBitisTarihi = reader["YetkiBitisTarihi"].ToString(),
                                YetkiGrubu = reader["YetkiGrubu"].ToString(),
                                SinirliYetkiDetaylari = reader["SinirliYetkiDetaylari"].ToString(),
                                YetkiTurleri = reader["YetkiTurleri"].ToString(),
                                YetkiTutari = Convert.ToDecimal(reader["YetkiTutari"]),
                                YetkiDovizCinsi = reader["YetkiDovizCinsi"].ToString(),
                                YetkiDurumu = reader["YetkiDurumu"].ToString()
                            };

                            currentCircular.Yetkililer.Add(yetkili);
                        }

                        // Son sirküler kaydını ekle
                        if (lastSirkulerNo != null)
                        {
                            results.Add(currentCircular);
                        }
                    }

                    return new CircularSearchResponse
                    {
                        Success = true,
                        Message = string.Format("Toplam {0} sirküler ve {1} yetkili bulundu.", 
                            results.Count, 
                            results.Sum(r => r.Yetkililer.Count)),
                        Data = results
                    };
                }
            }
        }
        catch (Exception ex)
        {
            return new CircularSearchResponse
            {
                Success = false,
                Message = string.Format("Hata oluştu: {0}", ex.Message),
                Data = new List<CircularSearchResult>()
            };
        }
    }
}
```

## Örnek Kullanım (C#)
```csharp
// Müşterinin tüm yetkilileri
var response1 = CircularDataManager.GetCustomerCircularAndYetkili("12345");

// Belirli bir yetkili
var response2 = CircularDataManager.GetCustomerCircularAndYetkili("12345", "5000711");

if (response2.Success)
{
    if (response2.Data.Any())
    {
        foreach (var circular in response2.Data)
        {
            Console.WriteLine(string.Format("Sirküler No: {0}", circular.Circular.CircularNotaryNo));
            Console.WriteLine(string.Format("Sirküler Tarihi: {0:dd.MM.yyyy}", circular.Circular.IssuedDate));
            
            foreach (var yetkili in circular.Yetkililer)
            {
                Console.WriteLine(string.Format("Yetkili: {0}", yetkili.YetkiliAdi));
                Console.WriteLine(string.Format("Yetki Bitiş: {0}", yetkili.YetkiBitisTarihi));
                
                if (yetkili.YetkiBitisTarihi != "Aksi Karara Kadar")
                {
                    DateTime bitisTarihi;
                    if (DateTime.TryParseExact(yetkili.YetkiBitisTarihi, "dd.MM.yyyy", null, 
                        System.Globalization.DateTimeStyles.None, out bitisTarihi))
                    {
                        if (bitisTarihi < DateTime.Today)
                        {
                            Console.WriteLine("Bu yetkilinin yetki süresi dolmuş!");
                        }
                    }
                }
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
    Console.WriteLine(string.Format("Hata: {0}", response2.Message));
}
```

## Notlar
- Stored procedure önce müşterinin aktif sirküleri olup olmadığını kontrol eder
- Sirküler varsa, yetkilileri listeler
- Yetkili kontak no verilmişse sadece o yetkiliyi filtreler
- Yetki süresi kontrolü otomatik yapılır
- Sadece aktif sirküler ve yetkilileri listeler
- Sonuçlar sirküler ve yetki tarihine göre sıralıdır
- Veriler `SignatureAuthData.cs`'deki sınıf yapısına uygun şekilde organize edilir
- String formatlama için string.Format kullanılır