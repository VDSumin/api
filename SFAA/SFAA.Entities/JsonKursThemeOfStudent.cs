using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SFAA.Entities
{
    using System.Runtime.Remoting.Messaging;

    /// <summary>
    /// Данная сущность предназначена для хранения информации по теме курсовой работе одного студента
    /// </summary>
    public class JsonKursThemeOfStudent
    {
        /// <summary>
        /// Nrec студента из таблицы U_MARKS
        /// </summary>
        [JsonIgnore]
        public byte[] MarkStudNrec
        {
            get;
            set;
        }

        /// <summary>
        /// Nrec студента строковый из таблицы U_MARKS
        /// </summary>
        [JsonProperty("markStudNrec")]
        public string MarkStudNrecString { get; set; } = "0x8000000000000000";

        /// <summary>
        /// ФИО студента
        /// </summary>
        [JsonProperty("fio")]
        public string Fio { get; set; } = string.Empty;
        
        /// <summary>
        /// Nrec студента из таблицы Persons
        /// </summary>
        [JsonIgnore]
        public byte[] StudPersonNrec
        {
            get;
            set;
        }

        /// <summary>
        /// Nrec студента строковый из таблицы Persons
        /// </summary>
        [JsonProperty("studPersonNrec")]
        public string StudPersonNrecString { get; set; } = "0x8000000000000000";

        /// <summary>
        /// Nrec записи курсовой работы в U_DB_Diploma
        /// </summary>
        [JsonIgnore]
        public byte[] DbDipNrec
        {
            get;
            set;
        }

        /// <summary>
        /// Nrec записи курсовой работы в U_DB_Diploma строковый
        /// </summary>
        [JsonProperty("dbDipNrec")]
        public string DbDipNrecString { get; set; } = "0x8000000000000000";


        /// <summary>
        /// Тема курсовой работы
        /// </summary>
        [JsonProperty("kursTheme")]
        public string KursTheme { get; set; }

        /// <summary>
        /// Дата редактирования темы курсовой работы
        /// </summary>
        [JsonProperty("kursThemeLastEdit")]
        public int KursThemeLastEdit { get; set; } = 0;
       
        /// <summary>
        /// Nrec руководителя курсовой работы
        /// </summary>
        [JsonIgnore]
        public byte[] KursThemeTeacherNrec
        {
            get;
            set;
        }

        /// <summary>
        /// Nrec руководителя курсовой работы строковый
        /// </summary>
        [JsonProperty("kursThemeTeacherNrec")]
        public string KursThemeTeacherNrecString { get; set; } = "0x8000000000000000";

        /// <summary>
        /// ФИО руководитиля курсовой работы
        /// </summary>
        [JsonProperty("kursThemeTeacherFio")]
        public string KursThemeTeacherFio { get; set; } = string.Empty;

    }
}
