using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace AspxExamples.Common.Models
{
    public class SignatureAuthDAL
    {
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

                return db.SetSpCommand("SGN.SelectYetkiliByCircular",
                    appParam.ToArray()
                ).ExecuteList<YetkiliData>();
            }
        }

        public static YetkiliData SelectYetkiliById(int id)
        {
            using (APPDb db = new APPDb())
            {
                return db.SetSpCommand("SGN.SelectYetkiliById",
                    db.Parameter("ID", id)
                ).ExecuteEntity<YetkiliData>();
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

                return db.SetSpCommand("SGN.SearchYetkili",
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

                return db.SetSpCommand("SGN.InsertYetkili",
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

                db.SetSpCommand("SGN.UpdateYetkili",
                    appParam.ToArray()
                ).ExecuteNonQuery();
            }
        }

        public static List<SignatureImage> SelectSignaturesByAuthDetail(int authDetailId)
        {
            using (APPDb db = new APPDb())
            {
                return db.SetSpCommand("SGN.SelectSignaturesByAuthDetail",
                    db.Parameter("AuthDetailID", authDetailId)
                ).ExecuteList<SignatureImage>();
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
                    db.Parameter("ImageData", Convert.FromBase64String(signature.ImageData)),
                    db.Parameter("SiraNo", signature.SiraNo)
                });

                if (!string.IsNullOrEmpty(signature.SourcePdfPath))
                {
                    appParam.Add(db.Parameter("SourcePdfPath", signature.SourcePdfPath));
                }

                return db.SetSpCommand("SGN.InsertSignature",
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
                    db.Parameter("ImageData", Convert.FromBase64String(signature.ImageData)),
                    db.Parameter("SiraNo", signature.SiraNo)
                });

                if (!string.IsNullOrEmpty(signature.SourcePdfPath))
                {
                    appParam.Add(db.Parameter("SourcePdfPath", signature.SourcePdfPath));
                }

                db.SetSpCommand("SGN.UpdateSignature",
                    appParam.ToArray()
                ).ExecuteNonQuery();
            }
        }

        #region Circular Operations
        public static CircularData SelectCircularById(int id)
        {
            using (APPDb db = new APPDb())
            {
                return db.SetSpCommand("SGN.SelectCircularById",
                    db.Parameter("ID", id)
                ).ExecuteEntity<CircularData>();
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

                return db.SetSpCommand("SGN.SelectCircularByCustomer",
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

                return db.SetSpCommand("SGN.SearchCircular",
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

                return db.SetSpCommand("SGN.InsertCircular",
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

                db.SetSpCommand("SGN.UpdateCircular",
                    appParam.ToArray()
                ).ExecuteNonQuery();
            }
        }

        public static CircularData GetCircularWithYetkililer(int id)
        {
            using (APPDb db = new APPDb())
            {
                var circular = db.SetSpCommand("SGN.SelectCircularById",
                    db.Parameter("ID", id)
                ).ExecuteEntity<CircularData>();

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
}