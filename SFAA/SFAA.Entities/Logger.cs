using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using log4net.Config;

namespace SFAA.Entities
{
    using System.Runtime.CompilerServices;

    public static class Logger
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Logger));

        public static ILog Log
        {
            get
            {
                return log;
            }
        }

        public static void InitLogger()
        {
            XmlConfigurator.Configure();
            Log.Debug($"Сервис запущен {DateTime.Now}");
        }
    }
}
