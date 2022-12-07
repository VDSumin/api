using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SFAA.Entities
{
    using System.Runtime.Remoting.Messaging;

    public class JsonTeacherList
    {
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
        [JsonProperty("numdoc")]
        public string NumDoc
        {
            get;
            set;
        }

        /// <summary>
        /// Год ведомости
        /// </summary>
        [JsonProperty("year")]
        public int Year
        {
            get;
            set;
        }

        /// <summary>
        /// Семестр ведомости (осень/весна)
        /// </summary>
        [JsonProperty("semester")]
        public string Semester
        {
            get;
            set;
        }

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
        /// Тип ведомости ввиде цифры
        /// </summary>
        [JsonProperty("typeListInt")]
        public int TypeListInt
        {
            get;
            set;
        }

        /// <summary>
        /// Статус ведомости 
        /// <seealso cref="ListStatus"/>
        /// </summary>
        [JsonProperty("status")]
        public ListStatus Status
        {
            get;
            set;
        }

        /// <summary>
        /// Форма обучения группы
        /// </summary>
        [JsonProperty("formEdu")]
        public int FormEdu
        {
            get;
            set;
        }

        /// <summary>
        /// Группа ведомости
        /// </summary>
        [JsonProperty("studGroup")]
        public string StudGroup
        {
            get;
            set;
        }

        /// <summary>
        /// Кафедра ведомости
        /// </summary>
        [JsonProperty("listChair")]
        public string ListChair
        {
            get;
            set;
        }

        /// <summary>
        /// Факультет ведомости
        /// </summary>
        [JsonProperty("listFacult")]
        public string ListFacult
        {
            get;
            set;
        }

        /// <summary>
        /// Дисциплина ведомости
        /// </summary>
        [JsonProperty("discipline")]
        public string Discipline
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
        [JsonProperty("examiner")]
        public string Examiner
        {
            get;
            set;
        }

        /// <summary>
        /// Дополнительный статус ведомости - проверяет возможность редактирования ведомости
        /// </summary>
        [JsonProperty("dopStatusList")]
        public int DopStatusList { get; set; } = 1;

        /// <summary>
        /// Количество студентов
        /// </summary>
        [JsonProperty("studentCount")]
        public int StudentCount { get; set; } = 0;

        /// <summary>
        /// Количество оценок
        /// </summary>
        [JsonProperty("markCount")]
        public int MarkCount { get; set; } = 0;

        /// <summary>
        /// ФИО студентов в направлении
        /// </summary>
        [JsonProperty("student")]
        public List<string> Student { get; set; } = new List<string>();
    }
}
