using System;
using System.Configuration;
using System.Net;
using System.Net.Mail;
using MessageQue.Models;

namespace MessageQue
{
    /// <summary>
    /// E-posta kuyruğu servisi.
    /// sendMailInQue: Mesajı hemen göndermez, MESAGEINQUE tablosuna ekler; sadece sabah 9 veya akşam 5'te cron job ile gönderilir.
    /// SendMail: Gerçek SMTP gönderimi (cron job tarafından kullanılır).
    /// </summary>
    public class MailQueueService
    {
        private readonly MessageInQueDAL _dal;
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;
        private readonly bool _enableSsl;
        private readonly string _defaultFromEmail;

        public MailQueueService()
        {
            _dal = new MessageInQueDAL();
            _smtpServer = ConfigurationManager.AppSettings["SmtpServer"] ?? "smtp.gmail.com";
            _smtpPort = int.Parse(ConfigurationManager.AppSettings["SmtpPort"] ?? "587");
            _smtpUsername = ConfigurationManager.AppSettings["SmtpUsername"] ?? "";
            _smtpPassword = ConfigurationManager.AppSettings["SmtpPassword"] ?? "";
            _enableSsl = bool.Parse(ConfigurationManager.AppSettings["EnableSsl"] ?? "true");
            _defaultFromEmail = ConfigurationManager.AppSettings["FromEmail"] ?? _smtpUsername;
        }

        /// <summary>
        /// E-postayı hemen göndermez; MESAGEINQUE tablosuna ekler.
        /// Mesaj sahiplerine sadece sabah 9 veya akşam 5'te (cron job ile) ulaşır.
        /// </summary>
        /// <param name="toEmail">Alıcı e-posta</param>
        /// <param name="subject">Konu</param>
        /// <param name="body">İçerik (HTML ise isBodyHtml true verin)</param>
        /// <param name="isBodyHtml">Body HTML mi</param>
        /// <param name="fromEmail">Gönderen (null ise config'deki FromEmail kullanılır)</param>
        /// <param name="cc">CC adresleri (virgülle ayrılmış, isteğe bağlı)</param>
        /// <param name="bcc">BCC adresleri (virgülle ayrılmış, isteğe bağlı)</param>
        /// <returns>Kuyruğa eklenen kaydın Id değeri</returns>
        public int SendMailInQue(string toEmail, string subject, string body = null, bool isBodyHtml = true, string fromEmail = null, string cc = null, string bcc = null)
        {
            if (string.IsNullOrWhiteSpace(toEmail))
                throw new ArgumentException("ToEmail boş olamaz.", nameof(toEmail));
            if (string.IsNullOrWhiteSpace(subject))
                throw new ArgumentException("Subject boş olamaz.", nameof(subject));

            return _dal.Insert(toEmail, subject, body, isBodyHtml, fromEmail ?? _defaultFromEmail, cc, bcc);
        }

        /// <summary>
        /// SMTP ile e-posta gönderir. Cron job bu metodu kuyruktaki kayıtlar için çağırır.
        /// </summary>
        public bool SendMail(string toEmail, string subject, string body, bool isBodyHtml = true, string fromEmail = null, string cc = null, string bcc = null)
        {
            try
            {
                using (var client = new SmtpClient(_smtpServer, _smtpPort))
                {
                    client.EnableSsl = _enableSsl;
                    if (!string.IsNullOrEmpty(_smtpUsername))
                    {
                        client.Credentials = new NetworkCredential(_smtpUsername, _smtpPassword);
                    }

                    var mail = new MailMessage
                    {
                        From = new MailAddress(fromEmail ?? _defaultFromEmail ?? "noreply@localhost"),
                        Subject = subject ?? "",
                        Body = body ?? "",
                        IsBodyHtml = isBodyHtml
                    };
                    mail.To.Add(toEmail);

                    AddAddresses(mail.CC, cc);
                    AddAddresses(mail.Bcc, bcc);

                    client.Send(mail);
                }
                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"SendMail hatası: {ex.Message}", ex);
            }
        }

        private static void AddAddresses(MailAddressCollection collection, string addresses)
        {
            if (string.IsNullOrWhiteSpace(addresses)) return;
            foreach (var addr in addresses.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var trimmed = addr.Trim();
                if (!string.IsNullOrEmpty(trimmed))
                    collection.Add(trimmed);
            }
        }

        /// <summary>
        /// Cron job: Kuyruktaki bekleyen kayıtları alır ve SendMail ile gönderir; sonucu tabloda günceller.
        /// Sadece saat 9 veya 17'de çalıştırılmalıdır.
        /// </summary>
        public void ProcessPendingQueue()
        {
            var pending = _dal.GetPending();
            foreach (var item in pending)
            {
                try
                {
                    SendMail(item.ToEmail, item.Subject, item.Body, item.IsBodyHtml, item.FromEmail, item.Cc, item.Bcc);
                    _dal.Update(item.Id, MailQueueItem.StatusSent, DateTime.Now, null);
                }
                catch (Exception ex)
                {
                    _dal.Update(item.Id, MailQueueItem.StatusFailed, null, ex.Message);
                }
            }
        }
    }
}
