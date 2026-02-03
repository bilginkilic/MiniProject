using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using MessageQue.Models;

namespace MessageQue
{
    /// <summary>
    /// MESAGEINQUE tablosu için veri erişim katmanı.
    /// </summary>
    public class MessageInQueDAL
    {
        private readonly string _connectionString;

        public MessageInQueDAL()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["MessageQueDb"]?.ConnectionString
                ?? ConfigurationManager.AppSettings["ConnectionString"]
                ?? throw new InvalidOperationException("ConnectionString (MessageQueDb veya ConnectionString) tanımlı değil.");
        }

        /// <summary>
        /// Tüm kayıtları getirir (SP_MESAGEINQUE_GET_ALL).
        /// </summary>
        public List<MailQueueItem> GetAll()
        {
            var list = new List<MailQueueItem>();
            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand("SP_MESAGEINQUE_GET_ALL", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                conn.Open();
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                        list.Add(ReadItem(r));
                }
            }
            return list;
        }

        /// <summary>
        /// Id ile tek kayıt getirir (SP_MESAGEINQUE_GET_BY_ID).
        /// </summary>
        public MailQueueItem GetById(int id)
        {
            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand("SP_MESAGEINQUE_GET_BY_ID", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Id", id);
                conn.Open();
                using (var r = cmd.ExecuteReader())
                {
                    if (r.Read())
                        return ReadItem(r);
                }
            }
            return null;
        }

        /// <summary>
        /// Gönderilmeyi bekleyen kayıtları getirir (SP_MESAGEINQUE_GET_PENDING). Cron job için.
        /// </summary>
        public List<MailQueueItem> GetPending()
        {
            var list = new List<MailQueueItem>();
            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand("SP_MESAGEINQUE_GET_PENDING", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                conn.Open();
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                        list.Add(ReadItem(r));
                }
            }
            return list;
        }

        /// <summary>
        /// Yeni kayıt ekler (SP_MESAGEINQUE_INS). sendMailInQue bu metodu kullanır.
        /// </summary>
        public int Insert(string toEmail, string subject, string body = null, bool isBodyHtml = true, string fromEmail = null, string cc = null, string bcc = null)
        {
            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand("SP_MESAGEINQUE_INS", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ToEmail", (object)toEmail ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Subject", (object)subject ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Body", (object)body ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@IsBodyHtml", isBodyHtml);
                cmd.Parameters.AddWithValue("@FromEmail", (object)fromEmail ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Cc", (object)cc ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Bcc", (object)bcc ?? DBNull.Value);
                conn.Open();
                var result = cmd.ExecuteScalar();
                return result != null && result != DBNull.Value ? Convert.ToInt32(result) : 0;
            }
        }

        /// <summary>
        /// Kayıt günceller (SP_MESAGEINQUE_UPD). Gönderim sonrası Status, SentAt, ErrorMessage.
        /// </summary>
        public void Update(int id, byte? status = null, DateTime? sentAt = null, string errorMessage = null)
        {
            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand("SP_MESAGEINQUE_UPD", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@Status", (object)status ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@SentAt", (object)sentAt ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ErrorMessage", (object)errorMessage ?? DBNull.Value);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private static MailQueueItem ReadItem(SqlDataReader r)
        {
            return new MailQueueItem
            {
                Id = r.GetInt32(r.GetOrdinal("Id")),
                ToEmail = r.IsDBNull(r.GetOrdinal("ToEmail")) ? null : r.GetString(r.GetOrdinal("ToEmail")),
                Subject = r.IsDBNull(r.GetOrdinal("Subject")) ? null : r.GetString(r.GetOrdinal("Subject")),
                Body = r.IsDBNull(r.GetOrdinal("Body")) ? null : r.GetString(r.GetOrdinal("Body")),
                IsBodyHtml = !r.IsDBNull(r.GetOrdinal("IsBodyHtml")) && r.GetBoolean(r.GetOrdinal("IsBodyHtml")),
                FromEmail = r.IsDBNull(r.GetOrdinal("FromEmail")) ? null : r.GetString(r.GetOrdinal("FromEmail")),
                Cc = r.IsDBNull(r.GetOrdinal("Cc")) ? null : r.GetString(r.GetOrdinal("Cc")),
                Bcc = r.IsDBNull(r.GetOrdinal("Bcc")) ? null : r.GetString(r.GetOrdinal("Bcc")),
                Status = r.GetByte(r.GetOrdinal("Status")),
                CreatedAt = r.GetDateTime(r.GetOrdinal("CreatedAt")),
                SentAt = r.IsDBNull(r.GetOrdinal("SentAt")) ? (DateTime?)null : r.GetDateTime(r.GetOrdinal("SentAt")),
                ErrorMessage = r.IsDBNull(r.GetOrdinal("ErrorMessage")) ? null : r.GetString(r.GetOrdinal("ErrorMessage"))
            };
        }
    }
}
