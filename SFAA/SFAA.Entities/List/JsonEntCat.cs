using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net.Util;
using Newtonsoft.Json;

namespace SFAA.Entities
{
    using System.Runtime.Remoting.Messaging;

    public class JsonEntCat
    {
        /// <summary>
        /// Nrec 
        /// </summary>
        [JsonProperty("nrec")]
        public string NrecString
        {
            get;
            set;
        }

        /// <summary>
        /// Nrec  int64
        /// </summary>
        [JsonProperty("nrecint64")]
        public string NrecInt64
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("label")]
        public string Label { get; set; } = string.Empty;

        
    }
}
