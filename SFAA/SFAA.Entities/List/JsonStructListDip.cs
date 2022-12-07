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

    public class JsonStructListDip
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JsonStructList"/> class.
        /// </summary>
        public JsonStructListDip() 
        {
            this.Student = new List<JsonStudentOfListDip>();
            this.ListExaminer = new List<JsonListExaminerOfList>();
        }

        /// <summary>
        /// Nrec ведомости
        /// </summary>
        [JsonIgnore]
        public byte[] Nrec
        {
            get;
            set;
        }

        /// <summary>
        /// Nrec ведомости строковый
        /// </summary>
        [JsonProperty("nrec")]
        public string NrecString
        {
            get;
            set;
        }

        /// <summary>
        /// Nrec ведомости int64
        /// </summary>
        [JsonProperty("nrecint64")]
        public string NrecInt64
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
        /// Тип ведомости числовой
        /// </summary>
        [JsonProperty("typeList")]
        public int TypeList { get; set; } = 0;


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
        /// Nrec дисциплины ведомости
        /// </summary>
        [JsonIgnore]
        public byte[] DisciplineNrec { get; set; }

        /// <summary>
        /// Nrec дисциплины ведомости строковый
        /// </summary>
        [JsonProperty("disciplineNrec")]
        public string DisciplineNrecString { get; set; } = "0x8000000000000000";

        /// <summary>
        /// Статус ведомости 
        /// <seealso cref="ListStatus"/>
        /// </summary>
        [JsonProperty("status")]
        public ListStatus Status { get; set; } = ListStatus.NEW;

        /// <summary>
        /// Дисциплина ведомости
        /// </summary>
        [JsonProperty("disciplineAbbr")]
        public string DisciplineAbbr { get; set; } = string.Empty;

        /// <summary>
        /// Общее количество часов по ведомости
        /// </summary>
        [JsonProperty("audHoursTotalList")]
        public int AudHoursTotalList { get; set; } = 0;
        
        /// <summary>
        /// Форма аттестации
        /// </summary>
        [JsonProperty("formAttestationList")]
        public string FormAttestationList { get; set; } = string.Empty;

        /// <summary>
        /// Признак, что ведомость идет с оценкой
        /// </summary>
        [JsonProperty("typeDiffer")]
        public int TypeDiffer { get; set; } = 0;

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
        /// Дополнительный статус ведомости - проверяет возможность редактирования ведомости
        /// </summary>
        [JsonProperty("dopStatusList")]
        public int? DopStatusList { get; set; } = null;

        /// <summary>
        /// Студенты текущей ведомости
        /// </summary>
        [JsonProperty("student")]
        public List<JsonStudentOfListDip> Student
        {
            get;
            set;
        }

        /// <summary>
        /// Список всех экзаменаторов данной ведомости
        /// </summary>
        [JsonProperty("listexaminer")]
        public List<JsonListExaminerOfList> ListExaminer
        {
            get;
            set;
        }
    }
}
