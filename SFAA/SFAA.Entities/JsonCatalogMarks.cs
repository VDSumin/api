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
    /// Сущность представляет собой набор свойств для одной оценки
    /// </summary>
    public class JsonCatalogMarks
    {
        /// <summary>
        /// Nrec оценки
        /// </summary>
        [JsonIgnore]
        public byte[] Nrec { get; set; }

        /// <summary>
        /// Nrec оценки строковый строковый
        /// </summary>
        [JsonProperty("nrec")]
        public string NrecString { get; set; } = "0x8000000000000000";

        /// <summary>
        /// Наименование оценки
        /// </summary>
        [JsonProperty("nameMark")]
        public string NameMark { get; set; } = string.Empty;

        /// <summary>
        /// Код оценки
        /// </summary>
        [JsonProperty("codeMark")]
        public string CodeMark { get; set; } = String.Empty;

        /// <summary>
        /// Наименование общей группы оценки
        /// </summary>
        [JsonProperty("groupName")]
        public string GroupName { get; set; } = string.Empty;

        /// <summary>
        /// Nrec общей группы оценки
        /// </summary>
        [JsonIgnore]
        public byte[] GroupNameNrec { get; set; }

        /// <summary>
        /// Nrec общей группы оценки строковый
        /// </summary>
        [JsonProperty("groupNameNrec")]
        public string GroupNameNrecString { get; set; } = string.Empty;
    }
}
