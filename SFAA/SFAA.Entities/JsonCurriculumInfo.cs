using Newtonsoft.Json;

namespace SFAA.Entities
{
    public class JsonCurriculumInfo
    {
        /// <summary>
        /// Nrec из таблицы dbo.T$U_STUDENTS
        /// </summary>
        [JsonIgnore]
        public byte[] Nrec { get; set; }

        /// <summary>
        /// Nrec из таблицы  dbo.T$U_STUDENTS
        /// </summary>
        [JsonProperty("nrec")]
        public string NrecString { get; set; } = "0x8000000000000000";

        /// <summary>
        /// Название факультета бакалавриата
        /// </summary>
        [JsonProperty("faculty")]
        public string Faculty { get; set; } = string.Empty;

        /// <summary>
        /// Название специальности
        /// </summary>
        [JsonProperty("speciality")]
        public string Speciality { get; set; } = string.Empty;

        /// <summary>
        /// Дата начала обучения
        /// </summary>
        [JsonProperty("begin")]
        public string BeginDate { get; set; } = string.Empty;

        /// <summary>
        /// Дата окончания обучения
        /// </summary>
        [JsonProperty("end")]
        public string EndDate { get; set; } = string.Empty;
    }
}
