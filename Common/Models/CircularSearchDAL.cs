/* v1 - CircularSearchDAL - Müşteri ve yetkili arama DAL katmanı - 2024.01.17 */
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace AspxExamples.Common.Models
{
    public class CircularSearchDAL
    {
        private const string SCHEMA = "dbo.SGN_";

        public static CircularSearchResponse GetCustomerCircularAndYetkili(string musteriNo, string yetkiliKontaktNo = null)
        {
            try
            {
                using (APPDb db = new APPDb())
                {
                    List<IDbDataParameter> appParam = new List<IDbDataParameter>();
                    appParam.AddRange(new IDbDataParameter[]
                    {
                        db.Parameter("MusteriNo", musteriNo),
                        db.Parameter("YetkiliKontaktNo", yetkiliKontaktNo)
                    });

                    var results = new List<CircularSearchResult>();
                    var currentCircular = new CircularSearchResult();
                    string lastSirkulerNo = null;

                    using (var reader = db.SetSpCommand(
                        string.Format("{0}GET_CUSTOMER_CIRCULAR_BY_CONTACT", SCHEMA),
                        appParam.ToArray()
                    ).ExecuteReader())
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

        public static CircularSearchResponse GetCustomerCircularsByDateRange(
            string musteriNo, 
            DateTime? baslangicTarihi = null, 
            DateTime? bitisTarihi = null)
        {
            try
            {
                using (APPDb db = new APPDb())
                {
                    List<IDbDataParameter> appParam = new List<IDbDataParameter>();
                    appParam.AddRange(new IDbDataParameter[]
                    {
                        db.Parameter("MusteriNo", musteriNo)
                    });

                    if (baslangicTarihi.HasValue)
                    {
                        appParam.Add(db.Parameter("BaslangicTarihi", baslangicTarihi.Value));
                    }

                    if (bitisTarihi.HasValue)
                    {
                        appParam.Add(db.Parameter("BitisTarihi", bitisTarihi.Value));
                    }

                    var results = db.SetSpCommand(
                        string.Format("{0}CIRCULAR_SELECT_BY_CUSTOMER", SCHEMA),
                        appParam.ToArray()
                    ).ExecuteList<CircularData>();

                    return new CircularSearchResponse
                    {
                        Success = true,
                        Message = string.Format("Toplam {0} sirküler bulundu.", results.Count),
                        Data = results.Select(c => new CircularSearchResult { Circular = c }).ToList()
                    };
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

        public static CircularSearchResponse GetYetkiliByDateRange(
            string yetkiliKontaktNo, 
            DateTime? baslangicTarihi = null, 
            DateTime? bitisTarihi = null)
        {
            try
            {
                using (APPDb db = new APPDb())
                {
                    List<IDbDataParameter> appParam = new List<IDbDataParameter>();
                    appParam.AddRange(new IDbDataParameter[]
                    {
                        db.Parameter("YetkiliKontaktNo", yetkiliKontaktNo)
                    });

                    if (baslangicTarihi.HasValue)
                    {
                        appParam.Add(db.Parameter("BaslangicTarihi", baslangicTarihi.Value));
                    }

                    if (bitisTarihi.HasValue)
                    {
                        appParam.Add(db.Parameter("BitisTarihi", bitisTarihi.Value));
                    }

                    var results = db.SetSpCommand(
                        string.Format("{0}AUTHDETAIL_SEARCH", SCHEMA),
                        appParam.ToArray()
                    ).ExecuteList<YetkiliData>();

                    return new CircularSearchResponse
                    {
                        Success = true,
                        Message = string.Format("Toplam {0} yetkili bulundu.", results.Count),
                        Data = new List<CircularSearchResult>
                        {
                            new CircularSearchResult { Yetkililer = results }
                        }
                    };
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
}
