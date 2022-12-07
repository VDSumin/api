using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFAA.Entities
{
    public class JsonEnterprises
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
        /// Название предприятия
        /// </summary>
        [JsonProperty("label")]
        public string Label { get; set; } = string.Empty;

        /// <summary>
        /// Адрес предприятия
        /// </summary>
        [JsonProperty("address")]
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// Телефон предприятия
        /// </summary>
        [JsonProperty("telephon")]
        public string Telephon { get; set; } = string.Empty;

        /// <summary>
        /// Почта предприятия
        /// </summary>
        [JsonProperty("mail")]
        public string Mail { get; set; } = string.Empty;

        /// <summary>
        /// Сокращенное наименование предприятия
        /// </summary>
        [JsonProperty("short_name")]
        public string ShortName { get; set; } = string.Empty;

        /// <summary>
        /// ID предприятия
        /// </summary>
        [JsonProperty("id")]
        public string ID { get; set; } = string.Empty;

        /// <summary>
        /// Nrec адреса предприятия
        /// </summary>
        [JsonProperty("nrec_address")]
        public string NrecAddressString { get; set; } = "0x8000000000000000";
    }
}
