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
    }
}