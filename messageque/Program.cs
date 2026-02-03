using System;

namespace MessageQue
{
    /// <summary>
    /// Cron job olarak kullanım: Windows Görev Zamanlayıcı ile sabah 9 (09:00) ve akşam 5 (17:00) saatlerinde çalıştırın.
    /// Bu exe çalıştığında MESAGEINQUE tablosundaki bekleyen (Pending) kayıtları alır ve SendMail ile gönderir.
    /// </summary>
    internal static class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                var hour = DateTime.Now.Hour;
                // Sadece 9 ve 17 saatlerinde işlem yap (opsiyonel: dışarıdan saat kontrolü istemiyorsanız bu bloku kaldırıp her çalışmada kuyruğu işleyebilirsiniz)
                if (hour != 9 && hour != 17)
                {
                    Console.WriteLine($"Cron job sadece 09:00 ve 17:00'da çalışmalı. Şu an saat: {hour}:{DateTime.Now.Minute:D2}. Çıkılıyor.");
                    return;
                }

                var service = new MailQueueService();
                service.ProcessPendingQueue();
                Console.WriteLine("Kuyruk işlendi.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Hata: " + ex.Message);
                Environment.Exit(1);
            }
        }
    }
}
