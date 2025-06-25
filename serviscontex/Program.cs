using System;
using System.ServiceProcess;

namespace ServisContex
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                switch (args[0].ToLower())
                {
                    case "install":
                        InstallUtil.InstallService();
                        break;
                    case "uninstall":
                        InstallUtil.UninstallService();
                        break;
                    case "start":
                        InstallUtil.StartService();
                        break;
                    case "stop":
                        InstallUtil.StopService();
                        break;
                    default:
                        Console.WriteLine("Geçersiz parametre. Kullanım:");
                        Console.WriteLine("ServisContex.exe install    # Servisi yükle");
                        Console.WriteLine("ServisContex.exe uninstall  # Servisi kaldır");
                        Console.WriteLine("ServisContex.exe start      # Servisi başlat");
                        Console.WriteLine("ServisContex.exe stop       # Servisi durdur");
                        break;
                }
            }
            else
            {
                // Normal servis çalıştırma
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                    new WindowsService()
                };
                ServiceBase.Run(ServicesToRun);
            }
        }
    }
} 