using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

// v1 - SignatureAuthDAL.cs - SP isimleri veritabanındaki SP'lerle uyumlu hale getirildi
namespace AspxExamples.Common.Models
{
    public class SignatureAuthDAL
    {
        private const string SCHEMA = "dbo.SGN_";

        public static List<YetkiliData> SelectYetkiliByCircular(int circularId, string yetkiDurumu = null)
        {
            using (APPDb db = new APPDb())
            {
                List<IDbDataParameter> appParam = new List<IDbDataParameter>();
                appParam.AddRange(new IDbDataParameter[]
                {
                    db.Parameter("circularId", circularId),
                    db.Parameter("yetkiDurumu", yetkiDurumu)
                });

                return db.SetSpCommand(string.Format("{0}AUTHDETAIL_SELECT_BY_CIRCULAR", SCHEMA),
                    appParam.ToArray()
                ).ExecuteList<YetkiliData>();
            }
        }

        public static YetkiliData SelectYetkiliById(int id)
        {
            using (APPDb db = new APPDb())
            {
                return db.SetSpCommand(string.Format("{0}AUTHDETAIL_SELECT_BY_ID", SCHEMA),
                    db.Parameter("ID", id)
                ).ExecuteObject<YetkiliData>();
            }
        }

        public static List<YetkiliData> SearchYetkili(string yetkiliAdi = null, string yetkiGrubu = null, 
            string yetkiDurumu = null, DateTime? baslangicTarihi = null, DateTime? bitisTarihi = null)
        {
            using (APPDb db = new APPDb())
            {
                List<IDbDataParameter> appParam = new List<IDbDataParameter>();
                appParam.AddRange(new IDbDataParameter[]
                {
                    db.Parameter("yetkiliAdi", yetkiliAdi),
                    db.Parameter("yetkiGrubu", yetkiGrubu),
                    db.Parameter("yetkiDurumu", yetkiDurumu)
                });

                if (baslangicTarihi.HasValue)
                {
                    appParam.Add(db.Parameter("baslangicTarihi", baslangicTarihi.Value));
                }

                if (bitisTarihi.HasValue)
                {
                    appParam.Add(db.Parameter("bitisTarihi", bitisTarihi.Value));
                }

                return db.SetSpCommand(string.Format("{0}AUTHDETAIL_SEARCH", SCHEMA),
                    appParam.ToArray()
                ).ExecuteList<YetkiliData>();
            }
        }

        public static int InsertYetkili(YetkiliData yetkili)
        {
            using (APPDb db = new APPDb())
            {
                List<IDbDataParameter> appParam = new List<IDbDataParameter>();
                appParam.AddRange(new IDbDataParameter[]
                {
                    db.Parameter("CircularID", yetkili.CircularID),
                    db.Parameter("YetkiliKontakt", yetkili.YetkiliKontakt),
                    db.Parameter("YetkiliAdi", yetkili.YetkiliAdi),
                    db.Parameter("YetkiSekli", yetkili.YetkiSekli),
                    db.Parameter("YetkiTarihi", yetkili.YetkiTarihi),
                    db.Parameter("YetkiBitisTarihi", yetkili.YetkiBitisTarihi),
                    db.Parameter("YetkiGrubu", yetkili.YetkiGrubu),
                    db.Parameter("YetkiTurleri", yetkili.YetkiTurleri),
                    db.Parameter("YetkiTutari", yetkili.YetkiTutari),
                    db.Parameter("YetkiDovizCinsi", yetkili.YetkiDovizCinsi),
                    db.Parameter("YetkiDurumu", yetkili.YetkiDurumu)
                });

                if (!string.IsNullOrEmpty(yetkili.SinirliYetkiDetaylari))
                {
                    appParam.Add(db.Parameter("SinirliYetkiDetaylari", yetkili.SinirliYetkiDetaylari));
                }

                return db.SetSpCommand(string.Format("{0}AUTHDETAIL_INSERT", SCHEMA),
                    appParam.ToArray()
                ).ExecuteScalar<int>();
            }
        }

        public static void UpdateYetkili(YetkiliData yetkili)
        {
            using (APPDb db = new APPDb())
            {
                List<IDbDataParameter> appParam = new List<IDbDataParameter>();
                appParam.AddRange(new IDbDataParameter[]
                {
                    db.Parameter("ID", yetkili.ID),
                    db.Parameter("CircularID", yetkili.CircularID),
                    db.Parameter("YetkiliKontakt", yetkili.YetkiliKontakt),
                    db.Parameter("YetkiliAdi", yetkili.YetkiliAdi),
                    db.Parameter("YetkiSekli", yetkili.YetkiSekli),
                    db.Parameter("YetkiTarihi", yetkili.YetkiTarihi),
                    db.Parameter("YetkiBitisTarihi", yetkili.YetkiBitisTarihi),
                    db.Parameter("YetkiGrubu", yetkili.YetkiGrubu),
                    db.Parameter("YetkiTurleri", yetkili.YetkiTurleri),
                    db.Parameter("YetkiTutari", yetkili.YetkiTutari),
                    db.Parameter("YetkiDovizCinsi", yetkili.YetkiDovizCinsi),
                    db.Parameter("YetkiDurumu", yetkili.YetkiDurumu)
                });

                if (!string.IsNullOrEmpty(yetkili.SinirliYetkiDetaylari))
                {
                    appParam.Add(db.Parameter("SinirliYetkiDetaylari", yetkili.SinirliYetkiDetaylari));
                }

                db.SetSpCommand(string.Format("{0}AUTHDETAIL_UPDATE", SCHEMA),
                    appParam.ToArray()
                ).ExecuteNonQuery();
            }
        }

        public static List<SignatureImage> SelectSignaturesByAuthDetail(int authDetailId)
        {
            using (APPDb db = new APPDb())
            {
                var signatures = db.SetSpCommand(string.Format("{0}AUTHDETAIL_SIGNATURES_SELECT_BY_AUTHDETAIL", SCHEMA),!=
                    db.Parameter("AuthDetailID", authDetailId)
                ).ExecuteList<SignatureImage>();

                // Her imza için byte array'i Base64'e dönüştür
                foreach (var signature in signatures)
                {
                    if (signature.ImageData != null)
                    {
                        try
                        {
                            // ImageData'yı byte array olarak al ve Base64'e dönüştür
                            byte[] imageBytes = (byte[])signature.ImageData;
                            signature.ImageData = ConvertBytesToBase64(imageBytes);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine(string.Format("İmza dönüşüm hatası (ID: {0}): {1}", signature.ID, ex.Message));
                            signature.ImageData = null; // Dönüşüm başarısız olursa null yap
                        }
                    }
                }

                return signatures;
            }
        }

        public static int InsertSignature(SignatureImage signature)
        {
            using (APPDb db = new APPDb())
            {
                List<IDbDataParameter> appParam = new List<IDbDataParameter>();
                appParam.AddRange(new IDbDataParameter[]
                {
                    db.Parameter("AuthDetailID", signature.AuthDetailID),
                    db.Parameter("ImageData", ConvertBase64ToBytes(signature.ImageData)),
                    db.Parameter("SiraNo", signature.SiraNo)
                });

                if (!string.IsNullOrEmpty(signature.SourcePdfPath))
                {
                    appParam.Add(db.Parameter("SourcePdfPath", signature.SourcePdfPath));
                }

                return db.SetSpCommand(string.Format("{0}AUTHDETAIL_SIGNATURES_INSERT", SCHEMA),
                    appParam.ToArray()
                ).ExecuteScalar<int>();
            }
        }

        public static void UpdateSignature(SignatureImage signature)
        {
            using (APPDb db = new APPDb())
            {
                List<IDbDataParameter> appParam = new List<IDbDataParameter>();
                appParam.AddRange(new IDbDataParameter[]
                {
                    db.Parameter("ID", signature.ID),
                    db.Parameter("AuthDetailID", signature.AuthDetailID),
                    db.Parameter("ImageData", ConvertBase64ToBytes(signature.ImageData)),
                    db.Parameter("SiraNo", signature.SiraNo)
                });

                if (!string.IsNullOrEmpty(signature.SourcePdfPath))
                {
                    appParam.Add(db.Parameter("SourcePdfPath", signature.SourcePdfPath));
                }

                db.SetSpCommand(string.Format("{0}AUTHDETAIL_SIGNATURES_UPDATE", SCHEMA),
                    appParam.ToArray()
                ).ExecuteNonQuery();
            }
        }

        #region Circular Operations
        public static List<CircularData> GetAllActiveCircularsWithSignatures()
        {
            using (APPDb db = new APPDb())
            {
                return db.SetSpCommand(string.Format("{0}AUTHDETAIL_GET_WITH_SIGNATURES", SCHEMA))
                    .ExecuteList<CircularData>();
            }
        }

        public static List<CircularData> GetAllActiveCircularsWithDetails()
        {
            using (APPDb db = new APPDb())
            {
                return db.SetSpCommand(string.Format("{0}AUTHDETAIL_GET_ALL_ACTIVE", SCHEMA))
                    .ExecuteList<CircularData>();
            }
        }
        public static List<CircularData> GetAllActiveCirculars()
        {
            using (APPDb db = new APPDb())
            {
                return db.SetSpCommand(string.Format("{0}AUTHDETAIL_GET_ALL_ACTIVE", SCHEMA))
                    .ExecuteList<CircularData>();
            }
        }
        public static CircularData SelectCircularById(int id)
        {
            using (APPDb db = new APPDb())
            {
                return db.SetSpCommand(string.Format("{0}CIRCULAR_SEL_SP", SCHEMA),
                    db.Parameter("ID", id)
                ).ExecuteObject<CircularData>();
            }
        }

        public static List<CircularData> SelectCircularByCustomer(string customerNo, string status = null)
        {
            using (APPDb db = new APPDb())
            {
                List<IDbDataParameter> appParam = new List<IDbDataParameter>();
                appParam.AddRange(new IDbDataParameter[]
                {
                    db.Parameter("CustomerNo", customerNo),
                    db.Parameter("CircularStatus", status)
                });

                return db.SetSpCommand(string.Format("{0}CIRCULAR_SELECT_BY_CUSTOMER", SCHEMA),
                    appParam.ToArray()
                ).ExecuteList<CircularData>();
            }
        }

        public static List<CircularData> SearchCircular(
            string customerNo = null, 
            string companyTitle = null,
            string circularType = null,
            string circularStatus = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            using (APPDb db = new APPDb())
            {
                List<IDbDataParameter> appParam = new List<IDbDataParameter>();
                appParam.AddRange(new IDbDataParameter[]
                {
                    db.Parameter("CustomerNo", customerNo),
                    db.Parameter("CompanyTitle", companyTitle),
                    db.Parameter("CircularType", circularType),
                    db.Parameter("CircularStatus", circularStatus)
                });

                if (startDate.HasValue)
                {
                    appParam.Add(db.Parameter("StartDate", startDate.Value));
                }

                if (endDate.HasValue)
                {
                    appParam.Add(db.Parameter("EndDate", endDate.Value));
                }

                return db.SetSpCommand(string.Format("{0}CIRCULAR_SEARCH", SCHEMA),
                    appParam.ToArray()
                ).ExecuteList<CircularData>();
            }
        }

        public static int InsertCircular(CircularData circular)
        {
            using (APPDb db = new APPDb())
            {
                List<IDbDataParameter> appParam = new List<IDbDataParameter>();
                appParam.AddRange(new IDbDataParameter[]
                {
                    db.Parameter("CustomerNo", circular.CustomerNo),
                    db.Parameter("CompanyTitle", circular.CompanyTitle),
                    db.Parameter("IssuedDate", circular.IssuedDate),
                    db.Parameter("ValidityDate", circular.ValidityDate),
                    db.Parameter("InternalBylawsTsgDate", circular.InternalBylawsTsgDate),
                    db.Parameter("SpecialCases", circular.SpecialCases),
                    db.Parameter("CircularType", circular.CircularType),
                    db.Parameter("CircularNotaryNo", circular.CircularNotaryNo),
                    db.Parameter("Description", circular.Description),
                    db.Parameter("CircularStatus", circular.CircularStatus),
                    db.Parameter("IsABoardOfDirectorsDecisionRequired", circular.IsABoardOfDirectorsDecisionRequired),
                    db.Parameter("MainCircularDate", circular.MainCircularDate),
                    db.Parameter("MainCircularRef", circular.MainCircularRef),
                    db.Parameter("AdditionalDocuments", circular.AdditionalDocuments),
                    db.Parameter("Channel", circular.Channel)
                });

                return db.SetSpCommand(string.Format("{0}CIRCULAR_INS_SP", SCHEMA),
                    appParam.ToArray()
                ).ExecuteScalar<int>();
            }
        }

        public static void UpdateCircular(CircularData circular)
        {
            using (APPDb db = new APPDb())
            {
                List<IDbDataParameter> appParam = new List<IDbDataParameter>();
                appParam.AddRange(new IDbDataParameter[]
                {
                    db.Parameter("ID", circular.ID),
                    db.Parameter("CustomerNo", circular.CustomerNo),
                    db.Parameter("CompanyTitle", circular.CompanyTitle),
                    db.Parameter("IssuedDate", circular.IssuedDate),
                    db.Parameter("ValidityDate", circular.ValidityDate),
                    db.Parameter("InternalBylawsTsgDate", circular.InternalBylawsTsgDate),
                    db.Parameter("SpecialCases", circular.SpecialCases),
                    db.Parameter("CircularType", circular.CircularType),
                    db.Parameter("CircularNotaryNo", circular.CircularNotaryNo),
                    db.Parameter("Description", circular.Description),
                    db.Parameter("CircularStatus", circular.CircularStatus),
                    db.Parameter("IsABoardOfDirectorsDecisionRequired", circular.IsABoardOfDirectorsDecisionRequired),
                    db.Parameter("MainCircularDate", circular.MainCircularDate),
                    db.Parameter("MainCircularRef", circular.MainCircularRef),
                    db.Parameter("AdditionalDocuments", circular.AdditionalDocuments),
                    db.Parameter("Channel", circular.Channel)
                });

                db.SetSpCommand(string.Format("{0}CIRCULAR_UPD_SP", SCHEMA),
                    appParam.ToArray()
                ).ExecuteNonQuery();
            }
        }

        public static CircularData GetCircularWithYetkililer(int id)
        {
            using (APPDb db = new APPDb())
            {
                var circular = db.SetSpCommand(string.Format("{0}AUTHDETAIL_SELECT_BY_ID", SCHEMA),
                    db.Parameter("ID", id)
                ).ExecuteObject<CircularData>();

                if (circular != null)
                {
                    circular.Yetkililer = SelectYetkiliByCircular(id);
                    
                    // İmzaları yükle
                    foreach (var yetkili in circular.Yetkililer)
                    {
                        yetkili.Imzalar = SelectSignaturesByAuthDetail(yetkili.ID);
                    }
                }

                return circular;
            }
        }
        #endregion
    }

    private static byte[] ConvertBase64ToBytes(string base64String)
    {
        if (string.IsNullOrEmpty(base64String))
            return null;

        // Remove data URI prefix if exists
        string cleanBase64 = base64String;
        if (base64String.Contains(","))
        {
            cleanBase64 = base64String.Split(',')[1];
        }

        // Clean the string
        cleanBase64 = cleanBase64.Trim()
            .Replace(" ", "")
            .Replace("\r", "")
            .Replace("\n", "")
            .Replace("\t", "");

        // Ensure proper padding
        int mod4 = cleanBase64.Length % 4;
        if (mod4 > 0)
        {
            cleanBase64 = cleanBase64.PadRight(cleanBase64.Length + (4 - mod4), '=');
        }

        try
        {
            return Convert.FromBase64String(cleanBase64);
        }
        catch (Exception ex)
        {
            throw new Exception("Base64 string dönüşümü sırasında hata oluştu: " + ex.Message);
        }
    }

    private static string ConvertBytesToBase64(byte[] imageData)
    {
        if (imageData == null || imageData.Length == 0)
            return null;

        try
        {
            return Convert.ToBase64String(imageData);
        }
        catch (Exception ex)
        {
            throw new Exception("Byte array'den Base64'e dönüşüm sırasında hata oluştu: " + ex.Message);
        }
    }
}