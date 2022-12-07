using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SFAA.Entities
{
    using System.Runtime.Remoting.Messaging;

    public class JsonHistoryFioChange
    {
        [JsonIgnore]
        public byte[] PersonNrec { get; set; }

        /// <summary>
        /// Nrec из таблицы persons строковый 
        /// </summary>
        [JsonProperty("nrec")]
        public string PersonNrecString { get; set; } = "0x8000000000000000";

        /// <summary>
        /// Nrec из таблицы persons int64 
        /// </summary>
        [JsonProperty("nrecint64")]
        public Int64 PersonNrecStringInt64 { get; set; } = 0;

        /// <summary>
        /// Старое ФИО
        /// </summary>
        [JsonProperty("fioOld")]
        public string FioOld { get; set; }

        /// <summary>
        /// Новое ФИО
        /// </summary>
        [JsonProperty("fioNew")]
        public string FioNew { get; set; }

        /// <summary>
        /// Новое ФИО - дата изменения
        /// </summary>
        [JsonProperty("fioNewDateChange")]
        public string FioNewDateChange { get; set; }
    }
}