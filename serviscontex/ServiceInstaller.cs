using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

namespace ServisContex
{
    [RunInstaller(true)]
    public class ServiceInstaller : Installer
    {
        private System.ServiceProcess.ServiceInstaller serviceInstaller;
        private System.ServiceProcess.ServiceProcessInstaller serviceProcessInstaller;

        public ServiceInstaller()
        {
            serviceProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            serviceInstaller = new System.ServiceProcess.ServiceInstaller();

            // Service Account Information
            serviceProcessInstaller.Account = ServiceAccount.LocalSystem;
            serviceProcessInstaller.Username = null;
            serviceProcessInstaller.Password = null;

            // Service Information
            serviceInstaller.ServiceName = "ServisContex";
            serviceInstaller.DisplayName = "ServisContex - SMTP Mail Service";
            serviceInstaller.Description = "SMTP mail servisi ile retry logic içeren Windows servisi";
            serviceInstaller.StartType = ServiceStartMode.Automatic;

            // Add installers to collection
            Installers.Add(serviceProcessInstaller);
            Installers.Add(serviceInstaller);
        }
    }
} 