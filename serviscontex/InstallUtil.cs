using System;
using System.Configuration.Install;
using System.Reflection;
using System.ServiceProcess;

namespace ServisContex
{
    public class InstallUtil
    {
        public static void InstallService()
        {
            try
            {
                ManagedInstallerClass.InstallHelper(new string[] { Assembly.GetExecutingAssembly().Location });
                Console.WriteLine("Servis başarıyla yüklendi.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Servis yükleme hatası: {ex.Message}");
            }
        }

        public static void UninstallService()
        {
            try
            {
                ManagedInstallerClass.InstallHelper(new string[] { "/u", Assembly.GetExecutingAssembly().Location });
                Console.WriteLine("Servis başarıyla kaldırıldı.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Servis kaldırma hatası: {ex.Message}");
            }
        }

        public static void StartService()
        {
            try
            {
                ServiceController sc = new ServiceController("ServisContex");
                if (sc.Status == ServiceControllerStatus.Stopped)
                {
                    sc.Start();
                    sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(30));
                    Console.WriteLine("Servis başarıyla başlatıldı.");
                }
                else
                {
                    Console.WriteLine("Servis zaten çalışıyor.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Servis başlatma hatası: {ex.Message}");
            }
        }

        public static void StopService()
        {
            try
            {
                ServiceController sc = new ServiceController("ServisContex");
                if (sc.Status == ServiceControllerStatus.Running)
                {
                    sc.Stop();
                    sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(30));
                    Console.WriteLine("Servis başarıyla durduruldu.");
                }
                else
                {
                    Console.WriteLine("Servis zaten durmuş.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Servis durdurma hatası: {ex.Message}");
            }
        }
    }
} 