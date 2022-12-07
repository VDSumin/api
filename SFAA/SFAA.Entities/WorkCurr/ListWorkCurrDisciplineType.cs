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
    public class ListWorkCurrDisciplineType
    {

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("comp")]
        public string comp { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("block")]
        public string block { get; set; }

    }
}
