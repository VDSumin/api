using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SFAA.Entities
{
    public class JsonPracticeList
    {
        /// <summary>
        /// Nrec из таблицы dbo.T$UP_REGISTER_PRACTICES
        /// </summary>
        [JsonIgnore]
        public byte[] Nrec { get; set; }

        /// <summary>
        /// Nrec из таблицы  dbo.T$UP_REGISTER_PRACTICES
        /// </summary>
        [JsonProperty("nrec")]
        public string NrecString { get; set; } = "0x8000000000000000";

        /// <summary>
        /// Nrec  int64
        /// </summary>
        [JsonProperty("nrecint64")]
        public Int64 NrecInt64 { get; set; } = 0;

        /// <summary>
        /// Название компании
        /// </summary>
        [JsonProperty("label")]
        public string Label { get; set; } = string.Empty;

        /// <summary>
        /// Nrec компании
        /// </summary>
        [JsonIgnore]
        public byte[] CompanyNrec { get; set; }

        /// <summary>
        /// Nrec компании in string 0x8 format
        /// </summary>
        [JsonProperty("companyNrec")]
        public string CompanyNrecString { get; set; } = "0x8000000000000000";

        /// <summary>
        /// Дата начала практики
        /// </summary>
        [JsonProperty("begin")]
        public string BeginDate { get; set; } = string.Empty;

        /// <summary>
        /// Дата окончания практики
        /// </summary>
        [JsonProperty("end")]
        public string EndDate { get; set; } = string.Empty;

        /// <summary>
        /// nrec of Person
        /// При сохранении изначально записываем T$U_MARKS.F$NREC для получения T$PERSONS.F$NREC
        /// </summary>
        [JsonIgnore]
        public byte[] PersonNrec { get; set; }

        /// <summary>
        /// nrec of Person in string 0x8 format
        /// При сохранении изначально записываем T$U_MARKS.F$NREC для получения T$PERSONS.F$NREC
        /// </summary>
        [JsonProperty("personNrec")]
        public string PersonNrecString { get; set; } = "0x8000000000000000";

        /// <summary>
        /// nrec ведомости
        /// </summary>
        [JsonIgnore]
        public byte[] ListNrec { get; set; }

        /// <summary>
        /// nrec ведомости 0x8
        /// </summary>
        [JsonProperty("listNrec")]
        public string ListNrecString { get; set; } = "0x8000000000000000";

        /// <summary>
        /// nrec преподавателя 
        /// </summary>
        [JsonIgnore]
        public byte[] ExaminerNrec { get; set; }

        /// <summary>
        /// nrec преподавателя 0x8
        /// </summary>
        [JsonProperty("examinerNrec")]
        public string ExaminerNrecString { get; set; } = "0x8000000000000000";

        /// <summary>
        /// Название практики
        /// </summary>
        [JsonProperty("discipline")]
        public string Discipline { get; set; } = string.Empty;

        /// <summary>
        /// Учебный год защиты практики
        /// </summary>
        [JsonProperty("yeared")]
        public Int32 Yeared { get; set; } = 0;
    }
}
