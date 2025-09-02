/* v1 - CircularDataManager - Müşteri ve yetkili arama business logic katmanı - 2024.01.17 */
using System;
using System.Linq;

namespace AspxExamples.Common.Models
{
    public static class CircularDataManager
    {
        public static CircularSearchResponse GetCustomerCircularAndYetkili(string musteriNo, string yetkiliKontaktNo = null)
        {
            try
            {
                // Parametre kontrolü
                if (string.IsNullOrEmpty(musteriNo))
                {
                    return new CircularSearchResponse
                    {
                        Success = false,
                        Message = "Müşteri numarası boş olamaz.",
                        Data = new List<CircularSearchResult>()
                    };
                }

                // DAL katmanını çağır
                var response = CircularSearchDAL.GetCustomerCircularAndYetkili(musteriNo, yetkiliKontaktNo);

                // Sonuçları işle
                if (response.Success && response.Data.Any())
                {
                    foreach (var result in response.Data)
                    {
                        // Yetki süresi kontrolü
                        foreach (var yetkili in result.Yetkililer)
                        {
                            if (yetkili.YetkiBitisTarihi != "Aksi Karara Kadar")
                            {
                                DateTime bitisTarihi;
                                if (DateTime.TryParseExact(yetkili.YetkiBitisTarihi, "dd.MM.yyyy", null,
                                    System.Globalization.DateTimeStyles.None, out bitisTarihi))
                                {
                                    if (bitisTarihi < DateTime.Today)
                                    {
                                        yetkili.YetkiDurumu = "Süresi Geçmiş";
                                    }
                                }
                            }
                        }
                    }
                }

                return response;
            }
            catch (Exception ex)
            {
                return new CircularSearchResponse
                {
                    Success = false,
                    Message = string.Format("İşlem sırasında hata oluştu: {0}", ex.Message),
                    Data = new List<CircularSearchResult>()
                };
            }
        }

        public static CircularSearchResponse GetCustomerCircularsByDateRange(
            string musteriNo,
            DateTime? baslangicTarihi = null,
            DateTime? bitisTarihi = null)
        {
            try
            {
                // Parametre kontrolü
                if (string.IsNullOrEmpty(musteriNo))
                {
                    return new CircularSearchResponse
                    {
                        Success = false,
                        Message = "Müşteri numarası boş olamaz.",
                        Data = new List<CircularSearchResult>()
                    };
                }

                // Tarih kontrolü
                if (baslangicTarihi.HasValue && bitisTarihi.HasValue)
                {
                    if (baslangicTarihi.Value > bitisTarihi.Value)
                    {
                        return new CircularSearchResponse
                        {
                            Success = false,
                            Message = "Başlangıç tarihi bitiş tarihinden büyük olamaz.",
                            Data = new List<CircularSearchResult>()
                        };
                    }
                }

                // DAL katmanını çağır
                return CircularSearchDAL.GetCustomerCircularsByDateRange(musteriNo, baslangicTarihi, bitisTarihi);
            }
            catch (Exception ex)
            {
                return new CircularSearchResponse
                {
                    Success = false,
                    Message = string.Format("İşlem sırasında hata oluştu: {0}", ex.Message),
                    Data = new List<CircularSearchResult>()
                };
            }
        }

        public static CircularSearchResponse GetYetkiliByDateRange(
            string yetkiliKontaktNo,
            DateTime? baslangicTarihi = null,
            DateTime? bitisTarihi = null)
        {
            try
            {
                // Parametre kontrolü
                if (string.IsNullOrEmpty(yetkiliKontaktNo))
                {
                    return new CircularSearchResponse
                    {
                        Success = false,
                        Message = "Yetkili kontak numarası boş olamaz.",
                        Data = new List<CircularSearchResult>()
                    };
                }

                // Tarih kontrolü
                if (baslangicTarihi.HasValue && bitisTarihi.HasValue)
                {
                    if (baslangicTarihi.Value > bitisTarihi.Value)
                    {
                        return new CircularSearchResponse
                        {
                            Success = false,
                            Message = "Başlangıç tarihi bitiş tarihinden büyük olamaz.",
                            Data = new List<CircularSearchResult>()
                        };
                    }
                }

                // DAL katmanını çağır
                return CircularSearchDAL.GetYetkiliByDateRange(yetkiliKontaktNo, baslangicTarihi, bitisTarihi);
            }
            catch (Exception ex)
            {
                return new CircularSearchResponse
                {
                    Success = false,
                    Message = string.Format("İşlem sırasında hata oluştu: {0}", ex.Message),
                    Data = new List<CircularSearchResult>()
                };
            }
        }
    }
}
