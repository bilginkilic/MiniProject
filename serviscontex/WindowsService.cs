using System;
using System.Configuration;
using System.Net.Mail;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace ServisContex
{
    public partial class WindowsService : ServiceBase
    {
        private System.Timers.Timer _timer;
        private readonly int _retryIntervalMinutes = 1; // Her 1 dakikada bir çalışır
        private readonly int _retryDelaySeconds = 60; // Her deneme arasında 60 saniye bekle
        
        // Gündüz/Gece tolerans ayarları
        private readonly int _dayMaxRetryAttempts; // Gündüz maksimum deneme sayısı
        private readonly int _nightMaxRetryAttempts; // Gece maksimum deneme sayısı
        private readonly int _dayStartHour; // Gündüz başlangıç saati
        private readonly int _dayEndHour; // Gündüz bitiş saati
        
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;
        private readonly bool _enableSsl;

        public WindowsService()
        {
            ServiceName = "ServisContex";
            
            // SMTP ayarlarını config'den oku
            _smtpServer = ConfigurationManager.AppSettings["SmtpServer"] ?? "smtp.gmail.com";
            _smtpPort = int.Parse(ConfigurationManager.AppSettings["SmtpPort"] ?? "587");
            _smtpUsername = ConfigurationManager.AppSettings["SmtpUsername"] ?? "";
            _smtpPassword = ConfigurationManager.AppSettings["SmtpPassword"] ?? "";
            _enableSsl = bool.Parse(ConfigurationManager.AppSettings["EnableSsl"] ?? "true");
            
            // Gündüz/Gece tolerans ayarlarını config'den oku
            _dayMaxRetryAttempts = int.Parse(ConfigurationManager.AppSettings["DayMaxRetryAttempts"] ?? "5");
            _nightMaxRetryAttempts = int.Parse(ConfigurationManager.AppSettings["NightMaxRetryAttempts"] ?? "45");
            _dayStartHour = int.Parse(ConfigurationManager.AppSettings["DayStartHour"] ?? "8");
            _dayEndHour = int.Parse(ConfigurationManager.AppSettings["DayEndHour"] ?? "18");
        }

        protected override void OnStart(string[] args)
        {
            LogMessage("Servis başlatılıyor...");
            
            _timer = new System.Timers.Timer();
            _timer.Interval = _retryIntervalMinutes * 60 * 1000; // Dakikayı milisaniyeye çevir
            _timer.Elapsed += Timer_Elapsed;
            _timer.AutoReset = true;
            _timer.Start();
            
            LogMessage("Servis başarıyla başlatıldı. Timer aktif.");
            LogMessage($"Gündüz toleransı: {_dayMaxRetryAttempts} dakika ({_dayStartHour}:00-{_dayEndHour}:00)");
            LogMessage($"Gece toleransı: {_nightMaxRetryAttempts} dakika ({_dayEndHour}:00-{_dayStartHour}:00)");
        }

        protected override void OnStop()
        {
            LogMessage("Servis durduruluyor...");
            
            if (_timer != null)
            {
                _timer.Stop();
                _timer.Dispose();
            }
            
            LogMessage("Servis durduruldu.");
        }

        private async void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            LogMessage("SMTP bağlantısı deneniyor...");
            
            bool success = await TrySmtpConnectionWithRetry();
            
            if (success)
            {
                LogMessage("SMTP bağlantısı başarılı. Mail işlemleri tamamlandı.");
            }
            else
            {
                int currentMaxAttempts = GetCurrentMaxRetryAttempts();
                LogMessage($"SMTP bağlantısı {currentMaxAttempts} dakika boyunca başarısız oldu. Servis durduruluyor.");
                // Servisi durdur, Task Scheduler tekrar başlatsın
                Stop();
            }
        }

        private int GetCurrentMaxRetryAttempts()
        {
            int currentHour = DateTime.Now.Hour;
            bool isDayTime = currentHour >= _dayStartHour && currentHour < _dayEndHour;
            
            if (isDayTime)
            {
                LogMessage($"Gündüz modu aktif - Tolerans: {_dayMaxRetryAttempts} dakika");
                return _dayMaxRetryAttempts;
            }
            else
            {
                LogMessage($"Gece modu aktif - Tolerans: {_nightMaxRetryAttempts} dakika");
                return _nightMaxRetryAttempts;
            }
        }

        private async Task<bool> TrySmtpConnectionWithRetry()
        {
            int maxRetryAttempts = GetCurrentMaxRetryAttempts();
            
            for (int attempt = 1; attempt <= maxRetryAttempts; attempt++)
            {
                LogMessage($"SMTP bağlantı denemesi {attempt}/{maxRetryAttempts}");
                
                try
                {
                    bool connectionSuccess = await TestSmtpConnection();
                    
                    if (connectionSuccess)
                    {
                        LogMessage($"SMTP bağlantısı başarılı (Deneme {attempt})");
                        return true;
                    }
                    else
                    {
                        LogMessage($"SMTP bağlantısı başarısız (Deneme {attempt})");
                    }
                }
                catch (Exception ex)
                {
                    LogMessage($"SMTP bağlantı hatası (Deneme {attempt}): {ex.Message}");
                }

                // Son deneme değilse bekle
                if (attempt < maxRetryAttempts)
                {
                    LogMessage($"{_retryDelaySeconds} saniye bekleniyor...");
                    await Task.Delay(_retryDelaySeconds * 1000);
                }
            }

            LogMessage($"Tüm SMTP bağlantı denemeleri başarısız oldu. ({maxRetryAttempts} deneme)");
            return false;
        }

        private async Task<bool> TestSmtpConnection()
        {
            return await Task.Run(() =>
            {
                try
                {
                    using (var client = new SmtpClient(_smtpServer, _smtpPort))
                    {
                        client.EnableSsl = _enableSsl;
                        client.Credentials = new System.Net.NetworkCredential(_smtpUsername, _smtpPassword);
                        client.Timeout = 30000; // 30 saniye timeout

                        // Test mail gönderimi (gerçek gönderim yapmadan)
                        // Bu sadece bağlantıyı test eder
                        client.SendCompleted += (sender, e) =>
                        {
                            if (e.Error != null)
                            {
                                LogMessage($"SMTP test hatası: {e.Error.Message}");
                            }
                        };

                        // Bağlantıyı test et
                        client.SendAsync(new MailMessage(), null);
                        
                        // Kısa bir süre bekle
                        Thread.Sleep(5000);
                        
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    LogMessage($"SMTP bağlantı testi hatası: {ex.Message}");
                    return false;
                }
            });
        }

        private void LogMessage(string message)
        {
            string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
            Console.WriteLine(logMessage);
            
            // Windows Event Log'a da yazabilirsiniz
            // EventLog.WriteEntry(ServiceName, message, EventLogEntryType.Information);
        }
    }
} 