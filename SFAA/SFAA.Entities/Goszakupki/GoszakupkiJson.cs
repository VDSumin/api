using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net.Util;
using Newtonsoft.Json;

namespace SFAA.Entities
{
    /// <summary>
    /// 
    /// </summary>
    public class GoszakupkiJson
    {
        [JsonProperty("elements")]
        public List<Goszakupki> Elements
        {
            get;
            set;
        }

        [JsonProperty("hash")]
        public string Hash
        {
            get;
            set;
        }
    }
}
