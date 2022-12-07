using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SFAA.Entities
{
    using System.Runtime.Remoting.Messaging;

    public class JsonStudentOfList
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
        /// Номер зачетной книжки
        /// </summary>
        [JsonProperty("recordBookNumber")]
        public string RecordBookNumber { get; set; } = string.Empty;

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
        /// Nrec оценки для поле cmark
        /// </summary>
        [JsonIgnore]
        public byte[] MarkLinkNumberNrec
        {
            get;
            set;
        }

        /// <summary>
        /// Nrec оценки для поле cmark строковый
        /// </summary>
        [JsonProperty("markLinkNumberNrec")]
        public string MarkLinkNumberNrecString { get; set; } = "0x8000000000000000";

        /// <summary>
        /// Оценка текстом
        /// </summary>
        [JsonProperty("markString")]
        public string MarkString { get; set; } = string.Empty;

        /// <summary>
        /// Оценка числом
        /// </summary>
        [JsonProperty("markNumber")]
        public int MarkNumber { get; set; } = 0;

        /// <summary>
        /// Статус оценки
        /// </summary>
        [JsonProperty("markWendres")]
        public int MarkWendres { get; set; } = 0;

        /// <summary>
        /// Наличие зачетки
        /// </summary>
        [JsonProperty("recordBookExist")]
        public int RecordBookExist { get; set; } = 0;

        /// <summary>
        /// Nrec преподавателя оценки
        /// </summary>
        [JsonIgnore]
        public byte[] MarkExaminerNrec
        {
            get;
            set;
        }

        /// <summary>
        /// Nrec преподавателя оценки строковый
        /// </summary>
        [JsonProperty("makrExaminerNrec")]
        public string MarkExaminerNrecString { get; set; } = "0x8000000000000000";

        /// <summary>
        /// ФИО преподавателя оценки строковый
        /// </summary>
        [JsonProperty("makrExaminerFio")]
        public string MarkExaminerNrecFio { get; set; } = string.Empty;


        /// <summary>
        /// Количество часов, посещенных студентом
        /// </summary>
        [JsonProperty("totalStudHours")]
        public int TotalStudHours { get; set; } = 0;

        /// <summary>
        /// Процент, посещенных занятий студентом
        /// </summary>
        [JsonProperty("percent")]
        public int Percent { get; set; } = 0;

        /// <summary>
        /// Рейтинг студента
        /// </summary>
        [JsonProperty("rating")]
        public int Rating { get; set; } = 0;

        /// <summary>
        /// Рейтинг студента семестровый
        /// </summary>
        [JsonProperty("ratingsem")]
        public int? RatingSem { get; set; } = 0;

        /// <summary>
        /// Рейтинг студента аттестационный
        /// </summary>
        [JsonProperty("ratingatt")]
        public int? RatingAtt { get; set; } = 0;

        /// <summary>
        /// Рейтинг студента итоговый
        /// </summary>
        [JsonProperty("ratingres")]
        public int? RatingRes { get; set; } = 0;

        /// <summary>
        /// Оценка студента из курсовой ведомости по аналогичной дисциплине и другим параметрам что и в основной ведомости
        /// </summary>
        [JsonProperty("markFromKursList")]
        public int? MarkFromKursList { get; set; } = 0;

        /// <summary>
        /// Nrec записи в таблице U_DB_DIPLOM. Поле необходимо для курсовых и дипломов
        /// </summary>
        [JsonIgnore]
        public byte[] DbDipNrec
        {
            get;
            set;
        }

        /// <summary>
        ///  Nrec записи в таблице U_DB_DIPLOM строковый. Поле необходимо для курсовых и дипломов
        /// </summary>
        /// <seealso cref="DbDipNrec"/>
        [JsonProperty("dbDipNrecNrec")]
        public string DbDipNrecString { get; set; } = "0x8000000000000000";

        /// <summary>
        /// Проверка наличия допуска
        /// </summary>
        [JsonProperty("tolerance")]
        public int? Tolerance { get; set; } = null;

        /// <summary>
        /// Тип ведомости от которой взята оценка
        /// </summary>
        [JsonProperty("markListType")]
        public int MarkListType { get; set; } = 0;

        /// <summary>
        /// Номер ведомости от которой взята оценка
        /// </summary>
        [JsonProperty("markListNumDoc")]
        public string MarkListNumDoc { get; set; } = string.Empty;
    }
}
