using System;
using System.IO;
using System.Threading.Tasks;

namespace Ctrix
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== Citrix PACO Uygulama Güncelleme Aracı ===");
            Console.WriteLine();

            if (args.Length == 0)
            {
                ShowUsage();
                return;
            }

            var manager = new CitrixAppManager();

            switch (args[0].ToLower())
            {
                case "update":
                    if (args.Length < 2)
                    {
                        Console.WriteLine("Hata: Yeni uygulama dosyası yolu belirtilmedi.");
                        Console.WriteLine("Kullanım: Ctrix.exe update <yeni_dosya_yolu>");
                        return;
                    }

                    var newAppPath = args[1];
                    if (!File.Exists(newAppPath))
                    {
                        Console.WriteLine($"Hata: Dosya bulunamadı: {newAppPath}");
                        return;
                    }

                    Console.WriteLine($"Güncelleme başlatılıyor: {newAppPath}");
                    var success = await manager.UpdateApplicationAsync(newAppPath);
                    
                    if (success)
                    {
                        Console.WriteLine("✅ Güncelleme başarıyla tamamlandı!");
                    }
                    else
                    {
                        Console.WriteLine("❌ Güncelleme başarısız oldu!");
                        Environment.Exit(1);
                    }
                    break;

                case "check":
                    Console.WriteLine("Aktif kullanıcılar kontrol ediliyor...");
                    var users = await manager.GetActiveUsersAsync();
                    
                    if (users.Any())
                    {
                        Console.WriteLine($"\n{users.Count} aktif kullanıcı bulundu:");
                        Console.WriteLine("Kullanıcı Adı | Oturum ID | Makine | Başlangıç Zamanı");
                        Console.WriteLine("-------------|-----------|--------|------------------");
                        
                        foreach (var user in users)
                        {
                            Console.WriteLine($"{user.Username,-12} | {user.SessionId,-9} | {user.MachineName,-6} | {user.StartTime:yyyy-MM-dd HH:mm}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("✅ Aktif kullanıcı bulunmuyor. Güvenle güncelleme yapılabilir.");
                    }
                    break;

                case "notify":
                    Console.WriteLine("Kullanıcılara uyarı gönderiliyor...");
                    var activeUsers = await manager.GetActiveUsersAsync();
                    await manager.SendUserNotificationsAsync(activeUsers);
                    Console.WriteLine("✅ Uyarılar gönderildi.");
                    break;

                case "cache":
                    Console.WriteLine("Citrix cache temizleniyor...");
                    await manager.ClearCitrixCacheAsync();
                    Console.WriteLine("✅ Cache temizlendi.");
                    break;

                case "republish":
                    Console.WriteLine("Uygulama yeniden yayınlanıyor...");
                    await manager.RepublishApplicationAsync();
                    Console.WriteLine("✅ Uygulama yeniden yayınlandı.");
                    break;

                default:
                    ShowUsage();
                    break;
            }
        }

        private static void ShowUsage()
        {
            Console.WriteLine("Kullanım:");
            Console.WriteLine("  Ctrix.exe update <yeni_dosya_yolu>  - Uygulamayı güncelle");
            Console.WriteLine("  Ctrix.exe check                     - Aktif kullanıcıları kontrol et");
            Console.WriteLine("  Ctrix.exe notify                    - Kullanıcılara uyarı gönder");
            Console.WriteLine("  Ctrix.exe cache                     - Cache'i temizle");
            Console.WriteLine("  Ctrix.exe republish                 - Uygulamayı yeniden yayınla");
            Console.WriteLine();
            Console.WriteLine("Örnek:");
            Console.WriteLine("  Ctrix.exe update C:\\Updates\\PACO_v2.1.exe");
        }
    }
} 