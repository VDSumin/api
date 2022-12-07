using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SFAA.Entities
{
    using System.Runtime.Remoting.Messaging;

    public class JsonStudentOfListDip
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
        public int? MarkNumber { get; set; } = 0;

        /// <summary>
        /// Статус оценки
        /// </summary>
        [JsonProperty("markWendres")]
        public int? MarkWendres { get; set; } = 0;

        /// <summary>
        /// Наличие зачетки
        /// </summary>
        [JsonProperty("recordBookExist")]
        public int? RecordBookExist { get; set; } = 0;
        
        /// <summary>
        /// Дата протокола для диплома
        /// </summary>
        [JsonProperty("dataProto")]
        public int? DataProto { get; set; } = 0;

        /// <summary>
        /// Номер протокола для диплома
        /// </summary>
        [JsonProperty("numberProto")]
        public string NumberProto { get; set; } = string.Empty;

        /// <summary>
        /// Наименование темы диплома
        /// </summary>
        [JsonProperty("titleDip")]
        public string TitleDip { get; set; } = string.Empty;

        /// <summary>
        /// Ссылка на диплом из карточки обучения
        /// </summary>
        [JsonIgnore]
        public byte[] EduNrec { get; set; }

        /// <summary>
        /// Ссылка на диплом из карточки обучения
        /// </summary>
        [JsonProperty("eduNrec")]
        public string EduNrecString { get; set; }

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
        /// Nrec приказа на допуск
        /// </summary>
        [JsonIgnore]
        public byte[] ToleranceNrec { get; set; } = null;

        /// <summary>
        /// Nrec приказа на допуск строковый
        /// </summary>
        [JsonProperty("toleranceNrec")]
        public string ToleranceNrecString { get; set; } = null;

        /// <summary>
        /// Тип ведомости от которой взята оценка
        /// </summary>
        [JsonProperty("markListType")]
        public int? MarkListType { get; set; } = 0;

        /// <summary>
        /// Номер ведомости от которой взята оценка
        /// </summary>
        [JsonProperty("markListNumDoc")]
        public string MarkListNumDoc { get; set; } = string.Empty;
    }
}
