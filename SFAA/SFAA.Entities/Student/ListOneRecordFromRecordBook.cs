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
    public class ListOneRecordFromRecordBook
    {
        /// <summary>
        /// Nrec студента строковый из таблицы Persons
        /// </summary>
        [JsonProperty("nrecString")]
        public string NrecString { get; set; } = "0x8000000000000000";

        /// <summary>
        /// Nrec студента
        /// </summary>
        [JsonProperty("nrecint64")]
        public Int64 NrecInt64 { get; set; }

        /// <summary>
        /// ФИО студента
        /// </summary>
        [JsonProperty("fio")]
        public string Fio { get; set; }

        /// <summary>
        /// Дисциплина nrec
        /// </summary>
        [JsonProperty("disciplineNrecString")]
        public string DisciplineNrecString { get; set; } = "0x8000000000000000";

        /// <summary>
        /// Дисциплина
        /// </summary>
        [JsonProperty("discipline")]
        public string Discipline { get; set; }

        /// <summary>
        /// Группа студента
        /// </summary>
        [JsonProperty("groupNumder")]
        public string GroupNumder { get; set; }

        /// <summary>
        /// Курс
        /// </summary>
        [JsonProperty("course")]
        public string Course { get; set; }

        /// <summary>
        /// Семестр
        /// </summary>
        [JsonProperty("semester")]
        public string Semester { get; set; }

        /// <summary>
        /// Тип работы
        /// </summary>
        [JsonProperty("typeOfWork")]
        public string TypeOfWork { get; set; }

        /// <summary>
        /// Количество часов из плана
        /// </summary>
        [JsonProperty("hoursOfPlan")]
        public string HoursOfPlan { get; set; }

        /// <summary>
        /// Тип ведомости
        /// </summary>
        [JsonProperty("listType")]
        public string ListType { get; set; }

        /// <summary>
        /// Номер ведомости
        /// </summary>
        [JsonProperty("numdoc")]
        public string Numdoc { get; set; }

        /// <summary>
        /// Статус оценки
        /// </summary>
        [JsonProperty("markStatus")]
        public string MarkStatus { get; set; }

        /// <summary>
        /// Рейтинг контрольной недели
        /// </summary>
        [JsonProperty("rcw")]
        public string Rcw { get; set; }

        /// <summary>
        /// Рейтинг итоговый
        /// </summary>
        [JsonProperty("r")]
        public string R { get; set; }

        /// <summary>
        /// Оценка
        /// </summary>
        [JsonProperty("mark")]
        public string Mark { get; set; }

        /// <summary>
        /// Информация об аттестации
        /// </summary>
        [JsonProperty("attestationInfo")]
        public string AttestationInfo { get; set; }

        /// <summary>
        /// Дата аттестации по ведомости
        /// </summary>
        [JsonProperty("attestationDate")]
        public string AttestationDate { get; set; }

        /// <summary>
        /// Дата оценки
        /// </summary>
        [JsonProperty("markDate")]
        public string MarkDate { get; set; }

        /// <summary>
        /// Экзаменатор
        /// </summary>
        [JsonProperty("examiner")]
        public string Examiner { get; set; }

        /// <summary>
        /// Отметка о дипломе
        /// </summary>
        [JsonProperty("listInDiplom")]
        public string ListInDiplom { get; set; }

        /// <summary>
        /// Допуск
        /// </summary>
        [JsonProperty("Toleran")]
        public string Toleran { get; set; }

        /// <summary>
        /// Факультатив
        /// </summary>
        [JsonProperty("facultative")]
        public string Facultative { get; set; }

        /// <summary>
        /// Признак выбранной дисциплины
        /// </summary>
        [JsonProperty("disciplineSelected")]
        public string DisciplineSelected { get; set; }

    }
}
