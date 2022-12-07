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
    /// Сущность представляет собой зачетную книжку
    /// </summary>
    public class JsonRecordBook
    {
        public JsonRecordBook()
        {
            this.Records = new List<ListOneRecordFromRecordBook>();
        }

        /// <summary>
        /// Nrec студента
        /// </summary>
        [JsonProperty("nrecint64")]
        public Int64 NrecInt64 { get; set; }

        /// <summary>
        /// Массив оценок из зачетной книжки
        /// </summary>
        [JsonProperty("record")]
        public List<ListOneRecordFromRecordBook> Records { get; set; }

    }
}
