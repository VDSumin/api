using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

    using SFAA.Business;
    using SFAA.Entities;

namespace SFAA.WindowsService
{
    
    public partial class SFAAService : ServiceBase
    {
        public SFAAService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Logger.Log.Info($"--------------------------------------------------");
            Logger.Log.Info($"Попытка запуска службы в {DateTime.Now.ToString()}");

            try
            {
                Logger.Log.Info($"Ожидаем клиента");
                var t = new Thread(new ThreadStart(MainBusiness.Instance.StartListener));
                t.Start();

            }
            catch (Exception exception)
            {
                Logger.Log.Error($"Ошибка при старте службы. Ошибка = {exception}");

            }
        }

        protected override void OnStop()
        {
            Logger.Log.Info($"Служба остановлена в {DateTime.Now.ToString()}");
            Logger.Log.Info($"--------------------------------------------------");
        }
    }
}
