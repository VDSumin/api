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
    /// Сущность представляет собой набор свойств для одного договора по общежитиям
    /// </summary>
    public class ListHostelContract
    {
        /// <summary>
        /// Nrec из таблицы persons
        /// </summary>
        [JsonIgnore]
        public byte[] NrecStudent { get; set; }

        /// <summary>
        /// Nrec из таблицы persons строковый 
        /// </summary>
        [JsonProperty("nrecStudent")]
        public string NrecStudentString { get; set; } = "0x8000000000000000";

        /// <summary>
        /// Nrec из таблицы persons int64 
        /// </summary>
        [JsonProperty("nrecStudentInt64")]
        public string NrecStudentStringInt64 { get; set; } = string.Empty;

        /// <summary>
        /// Id записи из таблицы hostel_contract
        /// </summary>
        [JsonProperty("id")]
        public Int32 Id { get; set; } = 0;

        /// <summary>
        /// Fnpp студента
        /// </summary>
        [JsonProperty("fnpp")]
        public Int32 Fnpp { get; set; } = 0;

        /// <summary>
        /// Дата договора
        /// </summary>
        [JsonProperty("contNumber")]
        public string ContNumber { get; set; } = string.Empty;

        /// <summary>
        /// Дата договора
        /// </summary>
        [JsonProperty("contDate")]
        public DateTime ContDate { get; set; }

        /// <summary>
        /// Приказ
        /// </summary>
        [JsonProperty("order")]
        public string Order { get; set; } = string.Empty;

        /// <summary>
        /// Дата договора
        /// </summary>
        [JsonProperty("orderDate")]
        public DateTime? OrderDate { get; set; }

        /// <summary>
        /// Дата начала договора
        /// </summary>
        [JsonProperty("contBegin")]
        public DateTime ContBegin { get; set; }

        /// <summary>
        /// Дата окончания договора
        /// </summary>
        [JsonProperty("contEnd")]
        public DateTime ContEnd { get; set; }

        /// <summary>
        /// Общежитие
        /// </summary>
        [JsonProperty("hostel")]
        public Int32 Hostel { get; set; } = 0;

        /// <summary>
        /// Общежитие адрес
        /// </summary>
        [JsonProperty("hostelAddress")]
        public string HostelAddress { get; set; }

        /// <summary>
        /// Блок
        /// </summary>
        [JsonProperty("block")]
        public string Block { get; set; }

        /// <summary>
        /// Комната
        /// </summary>
        [JsonProperty("flat")]
        public Int32 Flat { get; set; }

        /// <summary>
        /// Статус
        /// </summary>
        [JsonProperty("status")]
        public Int32 Status { get; set; }

        /// <summary>
        /// Причина
        /// </summary>
        [JsonProperty("reason")]
        public string Reason { get; set; } = string.Empty;

    }
}
