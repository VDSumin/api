using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SFAA.Entities
{
    using System.Runtime.Remoting.Messaging;

    public class GalOrderStudents30005
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
        [JsonProperty("dopCodRPD")]
        public string DopCodRPD { get; set; } = string.Empty;

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
        /// Источник финансирования
        /// </summary>
        [JsonProperty("finSourceCode")]
        public string FinSourceCode { get; set; } = string.Empty;

        /// <summary>
        /// Форма обучения
        /// </summary>
        [JsonProperty("formEdu")]
        public string FormEdu { get; set; } = string.Empty;

        /// <summary>
        /// Специальность после выхода
        /// </summary>
        [JsonProperty("spec")]
        public string Spec { get; set; } = string.Empty;

        /// <summary>
        /// План
        /// </summary>
        [JsonProperty("planStudy")]
        public string PlanStudy { get; set; } = string.Empty;

        /// <summary>
        /// Факультет после выхода
        /// </summary>
        [JsonProperty("facult")]
        public string Facult { get; set; } = string.Empty;

        // <summary>
        /// Номер договора
        /// </summary>
        [JsonProperty("dogovorNum1")]
        public string DogovorNum1 { get; set; } = string.Empty;

        /// <summary>
        /// Договор от
        /// </summary>
        [JsonProperty("dogovorFrom1")]
        public string DogovorFrom1 { get; set; } = string.Empty;

        // <summary>
        /// Договор до
        /// </summary>
        [JsonProperty("dogovorEnd1")]
        public string DogovorEnd1 { get; set; } = string.Empty;

        /// <summary>
        /// Дата нового назначения
        /// </summary>
        [JsonProperty("newAppDate")]
        public string NewAppDate { get; set; } = string.Empty;

        /// <summary>
        /// Новая Группа студента
        /// </summary>
        [JsonProperty("newStudentGroup")]
        public string NewStudentGroup { get; set; } = string.Empty;

        /// <summary>
        /// Новый Курс
        /// </summary>
        [JsonProperty("newStudentCourse")]
        public string NewStudentCourse { get; set; } = string.Empty;

        /// <summary>
        /// Новый Источник финансирования
        /// </summary>
        [JsonProperty("newFinSource")]
        public string NewFinSource { get; set; } = string.Empty;

        /// <summary>
        /// Новый Источник финансирования
        /// </summary>
        [JsonProperty("newFinSourceCode")]
        public string NewFinSourceCode { get; set; } = string.Empty;

        /// <summary>
        /// Новая Форма обучения
        /// </summary>
        [JsonProperty("newFormEdu")]
        public string NewFormEdu { get; set; } = string.Empty;

        /// <summary>
        /// Новая Специальность
        /// </summary>
        [JsonProperty("newSpec")]
        public string NewSpec { get; set; } = string.Empty;

        /// <summary>
        /// Новый факультет
        /// </summary>
        [JsonProperty("newFacult")]
        public string NewFacult { get; set; } = string.Empty;

        // <summary>
        /// Номер договора новый
        /// </summary>
        [JsonProperty("dogovorNum2")]
        public string DogovorNum2 { get; set; } = string.Empty;

        /// <summary>
        /// Договор от новый
        /// </summary>
        [JsonProperty("dogovorFrom2")]
        public string DogovorFrom2 { get; set; } = string.Empty;

        // <summary>
        /// Договор до новый
        /// </summary>
        [JsonProperty("dogovorEnd2")]
        public string DogovorEnd2 { get; set; } = string.Empty;
    }
}
