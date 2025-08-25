using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace AspxExamples.Common.Models
{
    public class SignatureAuthDAL : DbManager
    {
        #region Singleton
        private static SignatureAuthDAL instance;
        private static readonly object lockObject = new object();

        public static SignatureAuthDAL Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (lockObject)
                    {
                        if (instance == null)
                        {
                            instance = new SignatureAuthDAL();
                        }
                    }
                }
                return instance;
            }
        }

        private SignatureAuthDAL() : base() { }
        #endregion

        #region YetkiliData Methods
        public YetkiliData GetYetkiliById(int id)
        {
            using (SqlCommand cmd = AppDb.GetCommand("SP_SGN_AUTHDETAIL_SELECT_BY_ID"))
            {
                cmd.Parameters.AddWithValue("@ID", id);
                
                using (DataTable dt = AppDb.GetDataTable(cmd))
                {
                    if (dt.Rows.Count == 0) return null;
                    
                    return MapYetkiliDataFromRow(dt.Rows[0]);
                }
            }
        }

        public List<YetkiliData> GetYetkiliByCircular(int circularId)
        {
            using (SqlCommand cmd = AppDb.GetCommand("SP_SGN_AUTHDETAIL_SELECT_BY_CIRCULAR"))
            {
                cmd.Parameters.AddWithValue("@CircularID", circularId);
                
                using (DataTable dt = AppDb.GetDataTable(cmd))
                {
                    return MapYetkiliDataList(dt);
                }
            }
        }

        public List<YetkiliData> SearchYetkili(string yetkiliAdi = null, string yetkiGrubu = null, 
            string yetkiDurumu = null, DateTime? baslangicTarihi = null, DateTime? bitisTarihi = null)
        {
            using (SqlCommand cmd = AppDb.GetCommand("SP_SGN_AUTHDETAIL_SEARCH"))
            {
                if (!string.IsNullOrEmpty(yetkiliAdi))
                    cmd.Parameters.AddWithValue("@YetkiliAdi", yetkiliAdi);
                else
                    cmd.Parameters.AddWithValue("@YetkiliAdi", DBNull.Value);

                if (!string.IsNullOrEmpty(yetkiGrubu))
                    cmd.Parameters.AddWithValue("@YetkiGrubu", yetkiGrubu);
                else
                    cmd.Parameters.AddWithValue("@YetkiGrubu", DBNull.Value);

                if (!string.IsNullOrEmpty(yetkiDurumu))
                    cmd.Parameters.AddWithValue("@YetkiDurumu", yetkiDurumu);
                else
                    cmd.Parameters.AddWithValue("@YetkiDurumu", DBNull.Value);

                if (baslangicTarihi.HasValue)
                    cmd.Parameters.AddWithValue("@BaslangicTarihi", baslangicTarihi.Value);
                else
                    cmd.Parameters.AddWithValue("@BaslangicTarihi", DBNull.Value);

                if (bitisTarihi.HasValue)
                    cmd.Parameters.AddWithValue("@BitisTarihi", bitisTarihi.Value);
                else
                    cmd.Parameters.AddWithValue("@BitisTarihi", DBNull.Value);

                using (DataTable dt = AppDb.GetDataTable(cmd))
                {
                    return MapYetkiliDataList(dt);
                }
            }
        }

        public int InsertYetkili(YetkiliData yetkili)
        {
            using (SqlCommand cmd = AppDb.GetCommand("SP_SGN_AUTHDETAIL_INSERT"))
            {
                cmd.Parameters.AddWithValue("@CircularID", yetkili.CircularID);
                cmd.Parameters.AddWithValue("@YetkiliKontakt", (object)yetkili.YetkiliKontakt ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@YetkiliAdi", (object)yetkili.YetkiliAdi ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@YetkiSekli", (object)yetkili.YetkiSekli ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@YetkiTarihi", (object)yetkili.YetkiTarihi ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@YetkiBitisTarihi", (object)yetkili.YetkiBitisTarihi ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@YetkiGrubu", (object)yetkili.YetkiGrubu ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@SinirliYetkiDetaylari", (object)yetkili.SinirliYetkiDetaylari ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@YetkiTurleri", (object)yetkili.YetkiTurleri ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@YetkiTutari", yetkili.YetkiTutari);
                cmd.Parameters.AddWithValue("@YetkiDovizCinsi", (object)yetkili.YetkiDovizCinsi ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@YetkiDurumu", (object)yetkili.YetkiDurumu ?? DBNull.Value);

                return Convert.ToInt32(AppDb.ExecuteScalar(cmd));
            }
        }

        public void UpdateYetkili(YetkiliData yetkili)
        {
            using (SqlCommand cmd = AppDb.GetCommand("SP_SGN_AUTHDETAIL_UPDATE"))
            {
                cmd.Parameters.AddWithValue("@ID", yetkili.ID);
                cmd.Parameters.AddWithValue("@CircularID", yetkili.CircularID);
                cmd.Parameters.AddWithValue("@YetkiliKontakt", (object)yetkili.YetkiliKontakt ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@YetkiliAdi", (object)yetkili.YetkiliAdi ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@YetkiSekli", (object)yetkili.YetkiSekli ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@YetkiTarihi", (object)yetkili.YetkiTarihi ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@YetkiBitisTarihi", (object)yetkili.YetkiBitisTarihi ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@YetkiGrubu", (object)yetkili.YetkiGrubu ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@SinirliYetkiDetaylari", (object)yetkili.SinirliYetkiDetaylari ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@YetkiTurleri", (object)yetkili.YetkiTurleri ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@YetkiTutari", yetkili.YetkiTutari);
                cmd.Parameters.AddWithValue("@YetkiDovizCinsi", (object)yetkili.YetkiDovizCinsi ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@YetkiDurumu", (object)yetkili.YetkiDurumu ?? DBNull.Value);

                AppDb.ExecuteNonQuery(cmd);
            }
        }
        #endregion

        #region Signature Methods
        public List<SignatureImage> GetSignaturesByAuthDetail(int authDetailId)
        {
            using (SqlCommand cmd = AppDb.GetCommand("SP_SGN_AUTHDETAIL_SIGNATURES_SELECT_BY_AUTHDETAIL"))
            {
                cmd.Parameters.AddWithValue("@AuthDetailID", authDetailId);
                
                using (DataTable dt = AppDb.GetDataTable(cmd))
                {
                    return MapSignatureList(dt);
                }
            }
        }

        public int InsertSignature(SignatureImage signature)
        {
            using (SqlCommand cmd = AppDb.GetCommand("SP_SGN_AUTHDETAIL_SIGNATURES_INSERT"))
            {
                cmd.Parameters.AddWithValue("@AuthDetailID", signature.AuthDetailID);
                cmd.Parameters.AddWithValue("@ImageData", Convert.FromBase64String(signature.ImageData));
                cmd.Parameters.AddWithValue("@SiraNo", signature.SiraNo);
                cmd.Parameters.AddWithValue("@SourcePdfPath", (object)signature.SourcePdfPath ?? DBNull.Value);

                return Convert.ToInt32(AppDb.ExecuteScalar(cmd));
            }
        }

        public void UpdateSignature(SignatureImage signature)
        {
            using (SqlCommand cmd = AppDb.GetCommand("SP_SGN_AUTHDETAIL_SIGNATURES_UPDATE"))
            {
                cmd.Parameters.AddWithValue("@ID", signature.ID);
                cmd.Parameters.AddWithValue("@AuthDetailID", signature.AuthDetailID);
                cmd.Parameters.AddWithValue("@ImageData", Convert.FromBase64String(signature.ImageData));
                cmd.Parameters.AddWithValue("@SiraNo", signature.SiraNo);
                cmd.Parameters.AddWithValue("@SourcePdfPath", (object)signature.SourcePdfPath ?? DBNull.Value);

                AppDb.ExecuteNonQuery(cmd);
            }
        }
        #endregion

        #region Helper Methods
        private YetkiliData MapYetkiliDataFromRow(DataRow row)
        {
            return new YetkiliData
            {
                ID = Convert.ToInt32(row["ID"]),
                CircularID = Convert.ToInt32(row["CircularID"]),
                YetkiliKontakt = row["YetkiliKontakt"] as string,
                YetkiliAdi = row["YetkiliAdi"] as string,
                YetkiSekli = row["YetkiSekli"] as string,
                YetkiTarihi = row["YetkiTarihi"] as DateTime?,
                YetkiBitisTarihi = row["YetkiBitisTarihi"] as DateTime?,
                YetkiGrubu = row["YetkiGrubu"] as string,
                SinirliYetkiDetaylari = row["SinirliYetkiDetaylari"] as string,
                YetkiTurleri = row["YetkiTurleri"] as string,
                YetkiTutari = row["YetkiTutari"] != DBNull.Value ? Convert.ToDecimal(row["YetkiTutari"]) : 0,
                YetkiDovizCinsi = row["YetkiDovizCinsi"] as string,
                YetkiDurumu = row["YetkiDurumu"] as string
            };
        }

        private List<YetkiliData> MapYetkiliDataList(DataTable dt)
        {
            List<YetkiliData> list = new List<YetkiliData>();
            foreach (DataRow row in dt.Rows)
            {
                list.Add(MapYetkiliDataFromRow(row));
            }
            return list;
        }

        private SignatureImage MapSignatureFromRow(DataRow row)
        {
            return new SignatureImage
            {
                ID = Convert.ToInt32(row["ID"]),
                AuthDetailID = Convert.ToInt32(row["AuthDetailID"]),
                ImageData = Convert.ToBase64String((byte[])row["ImageData"]),
                SiraNo = Convert.ToInt32(row["SiraNo"]),
                SourcePdfPath = row["SourcePdfPath"] as string
            };
        }

        private List<SignatureImage> MapSignatureList(DataTable dt)
        {
            List<SignatureImage> list = new List<SignatureImage>();
            foreach (DataRow row in dt.Rows)
            {
                list.Add(MapSignatureFromRow(row));
            }
            return list;
        }
        #endregion
    }
}
