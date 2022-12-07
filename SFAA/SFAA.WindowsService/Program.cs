using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using SFAA.Entities;
using SFAA.Business;

namespace SFAA.WindowsService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
#if DEBUG
            GlobalSettings.Instance.ListenPort = Properties.Settings.Default.ListenPortDebug;
            GlobalSettings.Instance.ServerIp = Properties.Settings.Default.ServerIpDebug;
#else
            GlobalSettings.Instance.ListenPort = Properties.Settings.Default.ListenPort;
            GlobalSettings.Instance.ServerIp = Properties.Settings.Default.ServerIp;
#endif

            GlobalSettings.Instance.DnsGal = Properties.Settings.Default.DnsGal;
            GlobalSettings.Instance.GalDb = Properties.Settings.Default.GalDb;
            GlobalSettings.Instance.GoszakupkiJsonFile = Properties.Settings.Default.GoszakupkiJsonFile;
            GlobalSettings.Instance.GoszakupkiPythonScript = Properties.Settings.Default.GoszakupkiPythonScript;
            GlobalSettings.Instance.PythonPath = Properties.Settings.Default.PythonPath;

            MainBusiness.Instance.TryInialize();

            Logger.InitLogger();

            Logger.Log.Info($"Текущие настройки  IP: {GlobalSettings.Instance.ServerIp} Port: {GlobalSettings.Instance.ListenPort}");

            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new SFAAService()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
