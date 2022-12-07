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
    /// Сущность представляет собой набор свойств для одной дисциплины
    /// </summary>
    public class ListDiscipline
    {
        /// <summary>
        /// Nrec из таблицы дисципилны
        /// </summary>
        [JsonIgnore]
        public byte[] Nrec { get; set; }

        /// <summary>
        /// Nrec из таблицы дисциплины строковый 
        /// </summary>
        [JsonProperty("nrec")]
        public string NrecString { get; set; } = "0x8000000000000000";

        /// <summary>
        /// Nrec из таблицы дисциплины int64 
        /// </summary>
        [JsonProperty("nrecint64")]
        public Int64 NrecInt64 { get; set; } = 0;

        /// <summary>
        /// Наименование дисциплины  
        /// </summary>
        [JsonProperty("discipline")]
        public string Discipline { get; set; } = string.Empty;

        /// <summary>
        /// Наименование дисциплины аббревиатура
        /// </summary>
        [JsonProperty("disciplineAbbr")]
        public string DisciplineAbbr { get; set; } = string.Empty;
    }
}
