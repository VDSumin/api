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
    /// Сущность представляет собой набор свойств для одного студента
    /// </summary>
    public class ListStudent
    {
        /// <summary>
        /// Nrec из таблицы persons
        /// </summary>
        [JsonIgnore]
        public byte[] Nrec { get; set; }

        /// <summary>
        /// Nrec из таблицы persons строковый 
        /// </summary>
        [JsonProperty("nrec")]
        public string NrecString { get; set; } = "0x8000000000000000";

        /// <summary>
        /// Nrec из таблицы persons int64 
        /// </summary>
        [JsonProperty("nrecint64")]
        public string NrecStringInt64 { get; set; } = string.Empty;

        /// <summary>
        /// Strtabn из таблицы persons  
        /// </summary>
        [JsonProperty("strtabn")]
        public string Strtabn { get; set; } = string.Empty;

        /// <summary>
        /// ФИО студента
        /// </summary>
        [JsonProperty("fio")]
        public string Fio { get; set; } = string.Empty;

        /// <summary>
        /// Пол студента
        /// </summary>
        [JsonProperty("sex")]
        public string Sex { get; set; } = string.Empty;

        /// <summary>
        /// Дата рождения
        /// </summary>
        [JsonProperty("bornDate")]
        public string BornDate { get; set; } = string.Empty;

        /// <summary>
        /// Форма обучения
        /// </summary>
        [JsonProperty("formEdu")]
        public string FormEdu { get; set; } = string.Empty;

        /// <summary>
        /// Группа студента
        /// </summary>
        [JsonProperty("studGroup")]
        public string StudGroup { get; set; } = string.Empty;

        /// <summary>
        /// Статус студента
        /// </summary>
        [JsonProperty("status")]
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Факультет длинное название
        /// </summary>
        [JsonProperty("faculL")]
        public string FaculL { get; set; } = string.Empty;

        /// <summary>
        /// Факультет короткое название
        /// </summary>
        [JsonProperty("faculS")]
        public string FaculS { get; set; } = string.Empty;

        /// <summary>
        /// Курс студента
        /// </summary>
        [JsonProperty("course")]
        public short Course { get; set; } = 0;

        /// <summary>
        /// Специальность
        /// </summary>
        [JsonProperty("spec")]
        public string Spec { get; set; } = string.Empty;

        /// <summary>
        /// Квалификация
        /// </summary>
        [JsonProperty("Qual")]
        public string Qual { get; set; } = string.Empty;

        /// <summary>
        /// Источник финансирования
        /// </summary>
        [JsonProperty("finName")]
        public string FinName { get; set; } = string.Empty;

        /// <summary>
        /// Источник финансирования код
        /// </summary>
        [JsonProperty("finNameCode")]
        public string FinNameCode { get; set; } = string.Empty;

        /// <summary>
        /// Дата зачисления
        /// </summary>
        [JsonProperty("dateStudyStart")]
        public string DateStudyStart { get; set; } = string.Empty;

        /// <summary>
        /// Номер приказа о зачислении
        /// </summary>
        [JsonProperty("orderNumStudyStart")]
        public string OrderNumStudyStart { get; set; } = string.Empty;

        /// <summary>
        /// Дата приказа о зачислении
        /// </summary>
        [JsonProperty("orderDateStudyStart")]
        public string OrderDateStudyStart { get; set; } = string.Empty;

        /// <summary>
        /// Плановая дата окончания обучения
        /// </summary>
        [JsonProperty("planStudyEnd")]
        public string PlanStudyEnd { get; set; } = string.Empty;

        /// <summary>
        /// Номер договора
        /// </summary>
        [JsonProperty("dogovorNum")]
        public string DogovorNum { get; set; } = string.Empty;

        /// <summary>
        /// Дата договора
        /// </summary>
        [JsonProperty("dogovorDate")]
        public string DogovorDate { get; set; } = string.Empty;

        /// <summary>
        /// Номер зачетной книжки
        /// </summary>
        [JsonProperty("recordBook")]
        public string RecordBook { get; set; } = string.Empty;

        /// <summary>
        /// Данные для входа в прометей
        /// </summary>
        [JsonProperty("promAuth")]
        public string PromAuth { get; set; } = string.Empty;

        /// <summary>
        /// Данные по читательскому билету
        /// </summary>
        [JsonProperty("libTicket")]
        public string LibTicket { get; set; } = string.Empty;

    }
}
