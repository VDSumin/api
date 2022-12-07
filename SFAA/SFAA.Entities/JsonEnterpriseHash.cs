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
    /// Сущность представляет собой набор свойств места практики
    /// </summary>
    public class JsonEnterpriseHash
    {
        /// <summary>
        /// Hash
        /// </summary>
        [JsonProperty("hash")]
        public string Hash { get; set; }

        /// <summary>
        /// List jsonEntCat
        /// </summary>
        [JsonProperty("enterprises")]
        public List<JsonEntCat> Enterprises { get; set; }
    }
}
