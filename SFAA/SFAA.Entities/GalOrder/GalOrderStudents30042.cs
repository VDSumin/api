using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SFAA.Entities
{
    using System.Runtime.Remoting.Messaging;

    public class GalOrderStudents30042
    {
        [JsonIgnore]
        public byte[] PersonNrec { get; set; }

        /// <summary>
        /// Nrec из таблицы persons строковый 
        /// </summary>
        [JsonProperty("nrec")]
        public string PersonNrecString { get; set; } = "0x8000000000000000";

        /// <summary>
        /// Nrec из таблицы persons int64 
        /// </summary>
        [JsonProperty("nrecint64")]
        public Int64 PersonNrecStringInt64 { get; set; } = 0;

        /// <summary>
        /// Фио
        /// </summary>
        [JsonProperty("fioStudent")]
        public string FioStudent { get; set; } = string.Empty;

        /// <summary>
        /// Пол
        /// </summary>
        [JsonProperty("sex")]
        public string Sex { get; set; } = string.Empty;

        /// <summary>
        /// Фио в падеже
        /// </summary>
        [JsonProperty("fioStudentCaseChanging")]
        public string FioStudentCaseChanging { get; set; } = string.Empty;

        
        /// <summary>
        /// Гражданство
        /// </summary>
        [JsonProperty("gr")]
        public string Gr { get; set; } = string.Empty;

        /// <summary>
        /// Гражданство код
        /// </summary>
        [JsonProperty("grCode")]
        public string GrCode { get; set; } = string.Empty;

        /// <summary>
        /// Основание приказа по студенту
        /// </summary>
        [JsonProperty("basisOfOrder")]
        public string BasisOfOrder { get; set; } = string.Empty;

        /// <summary>
        /// Учетный номер
        /// </summary>
        [JsonProperty("strtabn")]
        public string Strtabn { get; set; } = string.Empty;

        /// <summary>
        /// Группа студента после выхода
        /// </summary>
        [JsonProperty("studentGroupAfter")]
        public string StudentGroupAfter { get; set; } = string.Empty;

        /// <summary>
        /// Курс после выхода
        /// </summary>
        [JsonProperty("studentCourseAfter")]
        public string StudentCourseAfter { get; set; } = string.Empty;

        /// <summary>
        /// Источник финансирования после выхода
        /// </summary>
        [JsonProperty("finSourceAfter")]
        public string FinSourceAfter { get; set; } = string.Empty;

        /// <summary>
        /// Форма обучения после выхода
        /// </summary>
        [JsonProperty("formEduAfter")]
        public string FormEduAfter { get; set; }

        /// <summary>
        /// Специальность после выхода
        /// </summary>
        [JsonProperty("specAfter")]
        public string SpecAfter { get; set; }
        
        /// <summary>
        /// Факультет после выхода
        /// </summary>
        [JsonProperty("facultAfter")]
        public string FacultAfter { get; set; }

       
        /// <summary>
        /// Дата начала назначения
        /// </summary>
        [JsonProperty("dateEnd")]
        public string DateEnd { get; set; }
        
        /// <summary>
        /// Номер договора
        /// </summary>
        [JsonProperty("dogovorNum")]
        public string DogovorNum{ get; set; }

        /// <summary>
        /// Договор от
        /// </summary>
        [JsonProperty("dogovorFrom")]
        public string DogovorFrom { get; set; }

        /// <summary>
        /// Договор до
        /// </summary>
        [JsonProperty("dogovorEnd")]
        public string DogovorEnd { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("parentOtpuskResom")]
        public string ParentOtpuskResom { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("parentDateDok")]
        public string ParentDateDok { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("parentNumDok")]
        public string ParentNumDok { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("parentOtpusk")]
        public string ParentOtpusk { get; set; }
    }
}
