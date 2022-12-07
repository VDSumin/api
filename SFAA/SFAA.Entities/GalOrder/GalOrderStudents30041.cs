﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SFAA.Entities
{
    using System.Runtime.Remoting.Messaging;

    public class GalOrderStudents30041
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
        /// Группа студента
        /// </summary>
        [JsonProperty("studentGroup")]
        public string StudentGroup { get; set; } = string.Empty;

        /// <summary>
        /// Курс
        /// </summary>
        [JsonProperty("studentCourse")]
        public string StudentCourse { get; set; } = string.Empty;

        /// <summary>
        /// Источник финансирования
        /// </summary>
        [JsonProperty("finSource")]
        public string FinSource { get; set; } = string.Empty;

        /// <summary>
        /// Форма обучения
        /// </summary>
        [JsonProperty("formEdu")]
        public string FormEdu { get; set; }

        /// <summary>
        /// Специальность после выхода
        /// </summary>
        [JsonProperty("spec")]
        public string Spec { get; set; }

        /// <summary>
        /// План
        /// </summary>
        [JsonProperty("planStudy")]
        public string PlanStudy { get; set; }

        /// <summary>
        /// Факультет после выхода
        /// </summary>
        [JsonProperty("facult")]
        public string Facult { get; set; }

       
        /// <summary>
        /// Наименование отпуска
        /// </summary>
        [JsonProperty("nOtpus")]
        public string NOtpus { get; set; }
        
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
        /// Дата начала отпуска
        /// </summary>
        [JsonProperty("otpuskFrom")]
        public string OtpuskFrom { get; set; }

        /// <summary>
        /// Дата окончания отпуска
        /// </summary>
        [JsonProperty("otpuskEnd")]
        public string OtpuskEnd { get; set; }

        /// <summary>
        /// Текст пролонгирования
        /// </summary>
        [JsonProperty("prolong")]
        public string Prolong { get; set; }
    }
}
