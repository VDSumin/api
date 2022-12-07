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

    public class JsonExtraListForStudent
    {
        /// <summary>
        /// Nrec студента
        /// </summary>
        [JsonIgnore]
        public byte[] PersonNrec
        {
            get;
            set;
        }

        /// <summary>
        /// Nrec студента строковый
        /// </summary>
        [JsonIgnore]
        public string PersonNrecString
        {
            get;
            set;
        }

        /// <summary>
        /// Номер ведомости
        /// </summary>
        [JsonProperty("numDoc")]
        public string NumDoc { get; set; } = string.Empty;
        
        /// <summary>
        /// Год ведомости
        /// </summary>
        [JsonProperty("year")]
        public int Year { get; set; } = 0;

        /// <summary>
        /// Тип ведомости
        /// </summary>
        [JsonProperty("typeList")]
        public string TypeListString
        {
            get;
            set;
        }

        /// <summary>
        /// Факультет ведомости
        /// </summary>
        [JsonProperty("listFacult")]
        public string ListFacult { get; set; } = string.Empty;

        /// <summary>
        /// Кафедра ведомости
        /// </summary>
        [JsonProperty("listChair")]
        public string ListChair { get; set; } = string.Empty;

        /// <summary>
        /// Группа ведомости
        /// </summary>
        [JsonProperty("studGroup")]
        public string StudGroup { get; set; } = string.Empty;

        /// <summary>
        /// Семестр ведомости
        /// </summary>
        [JsonProperty("semester")]
        public int Semester { get; set; } = 0;

        /// <summary>
        /// Дисциплина ведомости
        /// </summary>
        [JsonProperty("discipline")]
        public string Discipline { get; set; } = string.Empty;
        
        /// <summary>
        /// Дисциплина ведомости
        /// </summary>
        [JsonProperty("disciplineAbbr")]
        public string DisciplineAbbr { get; set; } = string.Empty;

       
        /// <summary>
        /// Дата ведомости
        /// </summary>
        [JsonProperty("dateList")]
        public int DateList
        {
            get;
            set;
        }

        /// <summary>
        /// Дата ведомости
        /// </summary>
        [JsonProperty("status")]
        public int Status
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
        /// Ответственный преподаватель ведомости
        /// </summary>
        [JsonProperty("lecturerFio")]
        public string LecturerFio { get; set; } = string.Empty;
    }
}
