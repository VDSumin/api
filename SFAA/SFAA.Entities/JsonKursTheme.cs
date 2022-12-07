using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net.Util;
using Newtonsoft.Json;

namespace SFAA.Entities
{
    using System.Runtime.Remoting.Messaging;

    /// <summary>
    /// Данная сущность предназначена для хранения информации по ведомости для курсовой работе
    /// </summary>
    public class JsonKursTheme
    {
        public JsonKursTheme()
        {
            this.Student = new List<JsonKursThemeOfStudent>();
            this.ListExaminer = new List<JsonListExaminerOfList>();
        }

        /// <summary>
        /// Nrec ведомости
        /// </summary>
        [JsonIgnore]
        public byte[] Nrec { get; set; }

        /// <summary>
        /// Nrec ведомости строковый
        /// </summary>
        [JsonProperty("nrec")]
        public string NrecString { get; set; }

        /// <summary>
        /// Nrec ведомости int64
        /// </summary>
        [JsonProperty("nrecint64")]
        public string NrecInt64 { get; set; }

        /// <summary>
        /// Номер ведомости
        /// </summary>
        [JsonProperty("numDoc")]
        public string NumDoc { get; set; } = string.Empty;


        /// <summary>
        /// Факультет ведомости
        /// </summary>
        [JsonProperty("listFacult")]
        public string ListFacult { get; set; } = string.Empty;


        /// <summary>
        /// Группа ведомости
        /// </summary>
        [JsonProperty("studGroup")]
        public string StudGroup { get; set; } = string.Empty;

        /// <summary>
        /// Дисциплина ведомости
        /// </summary>
        [JsonProperty("discipline")]
        public string Discipline { get; set; } = string.Empty;

        /// <summary>
        /// Статус ведомости 
        /// <seealso cref="ListStatus"/>
        /// </summary>
        [JsonProperty("status")]
        public ListStatus Status { get; set; } = ListStatus.NEW;

        /// <summary>
        /// Дополнительный статус ведомости - проверяет возможность редактирования ведомости
        /// </summary>
        [JsonProperty("dopStatusList")]
        public int? DopStatusList { get; set; } = null;

        /// <summary>
        /// Nrec ответственного
        /// </summary>
        [JsonIgnore]
        public byte[] ExaminerNrec
        {
            get;
            set;
        }

        /// <summary>
        /// Nrec ответственного строковый
        /// </summary>
        [JsonProperty("examinerNrec")]
        public string ExaminerNrecString
        {
            get;
            set;
        }

        /// <summary>
        /// Ответственный преподаватель ведомости
        /// </summary>
        [JsonProperty("examinerFio")]
        public string ExaminerFio { get; set; } = string.Empty;

        /// <summary>
        /// Студенты текущей ведомости
        /// </summary>
        [JsonProperty("student")]
        public List<JsonKursThemeOfStudent> Student { get; set; }

        /// <summary>
        /// Список всех экзаменаторов данной ведомости
        /// </summary>
        [JsonProperty("listexaminer")]
        public List<JsonListExaminerOfList> ListExaminer { get; set; }
    }
}
