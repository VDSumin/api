using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFAA.Entities
{
    public class JsonLecture
    {
        /// <summary>
        /// FNPP преподавателя
        /// </summary>
        [JsonProperty("fnpp")]
        public string Fnpp { get; set; }

        /// <summary>
        /// ФИО преподавателя
        /// </summary>
        [JsonProperty("fio")]
        public string Fio { get; set; } = string.Empty;

        /// <summary>
        /// Должность преподавателя
        /// </summary>
        [JsonProperty("position")]
        public string Position { get; set; } = string.Empty;
    }
}

