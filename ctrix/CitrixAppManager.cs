using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace Ctrix
{
    public class CitrixAppManager
    {
        private readonly string _appName = "PACO.Exe";
        private readonly string _citrixServerUrl;
        private readonly string _adminUsername;
        private readonly string _adminPassword;
        private readonly string _appPath;
        private readonly int _gracefulShutdownTimeout = 300; // 5 dakika

        public CitrixAppManager()
        {
            // Config'den ayarları oku
            _citrixServerUrl = ConfigurationManager.AppSettings["CitrixServerUrl"] ?? "";
            _adminUsername = ConfigurationManager.AppSettings["CitrixAdminUsername"] ?? "";
            _adminPassword = ConfigurationManager.AppSettings["CitrixAdminPassword"] ?? "";
            _appPath = ConfigurationManager.AppSettings["AppPath"] ?? @"C:\Applications\PACO.Exe";
        }

        /// <summary>
        /// Ana güncelleme işlemi
        /// </summary>
        public async Task<bool> UpdateApplicationAsync(string newAppPath)
        {
            try
            {
                LogMessage("Uygulama güncelleme işlemi başlatılıyor...");

                // 1. Aktif kullanıcıları kontrol et
                var activeUsers = await GetActiveUsersAsync();
                if (activeUsers.Any())
                {
                    LogMessage($"{activeUsers.Count} aktif kullanıcı bulundu. Graceful shutdown başlatılıyor...");
                    
                    // 2. Kullanıcılara uyarı gönder
                    await SendUserNotificationsAsync(activeUsers);
                    
                    // 3. Graceful shutdown - kullanıcıların uygulamayı kapatmasını bekle
                    if (!await WaitForGracefulShutdownAsync(activeUsers))
                    {
                        LogMessage("Graceful shutdown başarısız. Force shutdown başlatılıyor...");
                        await ForceShutdownApplicationsAsync(activeUsers);
                    }
                }

                // 4. Uygulamayı güncelle
                await UpdateApplicationFilesAsync(newAppPath);

                // 5. Cache'i temizle
                await ClearCitrixCacheAsync();

                // 6. Uygulamayı yeniden yayınla
                await RepublishApplicationAsync();

                LogMessage("Uygulama güncelleme işlemi tamamlandı.");
                return true;
            }
            catch (Exception ex)
            {
                LogMessage($"Uygulama güncelleme hatası: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Aktif kullanıcıları al
        /// </summary>
        public async Task<List<CitrixUser>> GetActiveUsersAsync()
        {
            var users = new List<CitrixUser>();

            try
            {
                // PowerShell ile Citrix kullanıcılarını al
                var script = @"
                    Add-PSSnapin Citrix*
                    Get-BrokerSession | Where-Object {$_.ApplicationName -eq 'PACO.Exe'} | 
                    Select-Object UserName, SessionId, MachineName, StartTime
                ";

                var result = await ExecutePowerShellAsync(script);
                
                // Sonuçları parse et
                var lines = result.Split('\n');
                foreach (var line in lines.Skip(1)) // Header'ı atla
                {
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        var parts = line.Split('|');
                        if (parts.Length >= 4)
                        {
                            users.Add(new CitrixUser
                            {
                                Username = parts[0].Trim(),
                                SessionId = parts[1].Trim(),
                                MachineName = parts[2].Trim(),
                                StartTime = DateTime.Parse(parts[3].Trim())
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Aktif kullanıcıları alma hatası: {ex.Message}");
            }

            return users;
        }

        /// <summary>
        /// Kullanıcılara uyarı gönder
        /// </summary>
        public async Task SendUserNotificationsAsync(List<CitrixUser> users)
        {
            foreach (var user in users)
            {
                try
                {
                    // Kullanıcıya popup mesajı gönder
                    var message = $"PACO uygulaması {_gracefulShutdownTimeout / 60} dakika içinde güncellenecektir. Lütfen çalışmanızı kaydedin ve uygulamayı kapatın.";
                    
                    var script = $@"
                        Add-PSSnapin Citrix*
                        Send-BrokerHostingPowerAction -SessionId {user.SessionId} -Action 'Message' -Message '{message}'
                    ";

                    await ExecutePowerShellAsync(script);
                    LogMessage($"Uyarı gönderildi: {user.Username}");
                }
                catch (Exception ex)
                {
                    LogMessage($"Uyarı gönderme hatası ({user.Username}): {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Graceful shutdown bekle
        /// </summary>
        private async Task<bool> WaitForGracefulShutdownAsync(List<CitrixUser> users)
        {
            var startTime = DateTime.Now;
            var timeout = TimeSpan.FromSeconds(_gracefulShutdownTimeout);

            while (DateTime.Now - startTime < timeout)
            {
                var remainingUsers = await GetActiveUsersAsync();
                
                if (!remainingUsers.Any())
                {
                    LogMessage("Tüm kullanıcılar uygulamayı kapattı.");
                    return true;
                }

                var remainingTime = timeout - (DateTime.Now - startTime);
                LogMessage($"{remainingUsers.Count} kullanıcı hala aktif. Kalan süre: {remainingTime:mm\\:ss}");

                await Task.Delay(10000); // 10 saniye bekle
            }

            LogMessage("Graceful shutdown süresi doldu.");
            return false;
        }

        /// <summary>
        /// Force shutdown uygula
        /// </summary>
        private async Task ForceShutdownApplicationsAsync(List<CitrixUser> users)
        {
            foreach (var user in users)
            {
                try
                {
                    var script = $@"
                        Add-PSSnapin Citrix*
                        Stop-BrokerSession -SessionId {user.SessionId} -Force
                    ";

                    await ExecutePowerShellAsync(script);
                    LogMessage($"Force shutdown uygulandı: {user.Username}");
                }
                catch (Exception ex)
                {
                    LogMessage($"Force shutdown hatası ({user.Username}): {ex.Message}");
                }
            }

            // Kısa bir bekleme
            await Task.Delay(5000);
        }

        /// <summary>
        /// Uygulama dosyalarını güncelle
        /// </summary>
        private async Task UpdateApplicationFilesAsync(string newAppPath)
        {
            try
            {
                LogMessage("Uygulama dosyaları güncelleniyor...");

                // Yedek al
                var backupPath = $"{_appPath}.backup.{DateTime.Now:yyyyMMddHHmmss}";
                if (File.Exists(_appPath))
                {
                    File.Copy(_appPath, backupPath);
                    LogMessage($"Yedek oluşturuldu: {backupPath}");
                }

                // Yeni dosyayı kopyala
                File.Copy(newAppPath, _appPath, true);
                LogMessage($"Uygulama güncellendi: {_appPath}");

                // Dosya hash'ini kontrol et
                var newHash = await CalculateFileHashAsync(_appPath);
                LogMessage($"Yeni dosya hash: {newHash}");
            }
            catch (Exception ex)
            {
                LogMessage($"Dosya güncelleme hatası: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Citrix cache'ini temizle
        /// </summary>
        public async Task ClearCitrixCacheAsync()
        {
            try
            {
                LogMessage("Citrix cache temizleniyor...");

                var script = @"
                    Add-PSSnapin Citrix*
                    
                    # Delivery Controller cache'ini temizle
                    Get-BrokerApplication | Where-Object {$_.Name -eq 'PACO.Exe'} | 
                    ForEach-Object {
                        $_.ClearCache()
                    }
                    
                    # StoreFront cache'ini temizle
                    Get-SFStore | ForEach-Object {
                        $_.ClearCache()
                    }
                ";

                await ExecutePowerShellAsync(script);
                LogMessage("Citrix cache temizlendi.");
            }
            catch (Exception ex)
            {
                LogMessage($"Cache temizleme hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Uygulamayı yeniden yayınla
        /// </summary>
        public async Task RepublishApplicationAsync()
        {
            try
            {
                LogMessage("Uygulama yeniden yayınlanıyor...");

                var script = @"
                    Add-PSSnapin Citrix*
                    
                    # Uygulamayı yeniden yayınla
                    Get-BrokerApplication | Where-Object {$_.Name -eq 'PACO.Exe'} | 
                    ForEach-Object {
                        $_.Publish()
                    }
                ";

                await ExecutePowerShellAsync(script);
                LogMessage("Uygulama yeniden yayınlandı.");
            }
            catch (Exception ex)
            {
                LogMessage($"Yeniden yayınlama hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// PowerShell script çalıştır
        /// </summary>
        private async Task<string> ExecutePowerShellAsync(string script)
        {
            return await Task.Run(() =>
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-Command \"{script}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(startInfo))
                {
                    var output = process.StandardOutput.ReadToEnd();
                    var error = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    if (!string.IsNullOrEmpty(error))
                    {
                        LogMessage($"PowerShell hatası: {error}");
                    }

                    return output;
                }
            });
        }

        /// <summary>
        /// Dosya hash hesapla
        /// </summary>
        private async Task<string> CalculateFileHashAsync(string filePath)
        {
            return await Task.Run(() =>
            {
                using (var md5 = System.Security.Cryptography.MD5.Create())
                using (var stream = File.OpenRead(filePath))
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            });
        }

        private void LogMessage(string message)
        {
            var logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
            Console.WriteLine(logMessage);
            
            // Dosyaya da yaz
            File.AppendAllText("CitrixAppManager.log", logMessage + Environment.NewLine);
        }
    }

    public class CitrixUser
    {
        public string Username { get; set; }
        public string SessionId { get; set; }
        public string MachineName { get; set; }
        public DateTime StartTime { get; set; }
    }
} 