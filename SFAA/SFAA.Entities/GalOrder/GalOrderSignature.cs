using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SFAA.Entities
{
    using System.Runtime.Remoting.Messaging;

    public class GalOrderSignature
    {
        /// <summary>
        /// Должность
        /// </summary>
        [JsonProperty("post")]
        public string Post { get; set; } = string.Empty;

        /// <summary>
        /// ФИО
        /// </summary>
        [JsonProperty("fio")]
        public string Fio { get; set; } = string.Empty;

        /// <summary>
        /// Порядок согласования
        /// </summary>
        [JsonProperty("prioritet")]
        public string Prioritet { get; set; } = string.Empty;
    }
}
