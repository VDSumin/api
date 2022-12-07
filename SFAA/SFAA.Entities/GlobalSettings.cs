using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFAA.Entities
{
    public class GlobalSettings
    {
        private static GlobalSettings instance;

        /// <summary>
        /// Gets the instance.
        /// </summary>
        public static GlobalSettings Instance => instance ?? (instance = new GlobalSettings());

        public Int32 ListenPort;

        public string ServerIp;

        public string ApiKeyUpOmGTURu;

        public string AuthDataGalLogin;

        public string AuthDataGalPass;

        public string DnsGal;

        public string GalDb;

        public string GoszakupkiPythonScript;

        public string GoszakupkiJsonFile;

        public string PythonPath;

        public ApiUsers CurrentApiUser;

    }
}
