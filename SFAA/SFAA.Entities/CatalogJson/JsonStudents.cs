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
    public class JsonStudents
    {
        /// <summary>
        /// Hash ListStudetn
        /// </summary>
        [JsonProperty("hash")]
        public string Hash { get; set; }

        /// <summary>
        /// ListStudent
        /// </summary>
        [JsonProperty("students")]
        public List<ListStudent> Students { get; set; }

    }
}
