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
    /// Сущность представляет собой набор свойств для одного студента
    /// </summary>
    public class ListEmployee
    {
        /// <summary>
        /// Nrec из таблицы persons
        /// </summary>
        [JsonIgnore]
        public byte[] Nrec { get; set; }

        /// <summary>
        /// Nrec из таблицы persons строковый 
        /// </summary>
        [JsonProperty("nrec")]
        public string NrecString { get; set; } = "0x8000000000000000";

        /// <summary>
        /// ФИО работника
        /// </summary>
        [JsonProperty("fio")]
        public string Fio { get; set; } = string.Empty;

        /// <summary>
        /// Nrec подраздееления
        /// </summary>
        [JsonIgnore]
        public byte[] DepNrec { get; set; }

        /// <summary>
        /// Nrec подраздееления строковый 
        /// </summary>
        [JsonProperty("depNrec")]
        public string DepNrecString { get; set; } = "0x8000000000000000";

        /// <summary>
        /// Наименование подраздееления
        /// </summary>
        [JsonProperty("dep")]
        public string Dep { get; set; } = string.Empty;

        /// <summary>
        /// Nrec должности
        /// </summary>
        [JsonIgnore]
        public byte[] PostNrec { get; set; }

        /// <summary>
        /// Nrec должности строковый 
        /// </summary>
        [JsonProperty("postNrec")]
        public string PostNrecString { get; set; } = "0x8000000000000000";

        /// <summary>
        /// Наименование должности
        /// </summary>
        [JsonProperty("post")]
        public string Post { get; set; } = string.Empty;


    }
}
