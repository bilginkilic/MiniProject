using System;

namespace MessageQue.Models
{
    /// <summary>
    /// MESAGEINQUE tablosuna karşılık gelen model.
    /// Status: 0=Pending, 1=Sent, 2=Failed
    /// </summary>
    public class MailQueueItem
    {
        public int Id { get; set; }
        public string ToEmail { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public bool IsBodyHtml { get; set; }
        public string FromEmail { get; set; }
        /// <summary>Virgülle ayrılmış CC adresleri</summary>
        public string Cc { get; set; }
        /// <summary>Virgülle ayrılmış BCC adresleri</summary>
        public string Bcc { get; set; }
        public byte Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? SentAt { get; set; }
        public string ErrorMessage { get; set; }

        public const byte StatusPending = 0;
        public const byte StatusSent = 1;
        public const byte StatusFailed = 2;
    }
}
