using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SFAA.Entities
{
    using System.Runtime.Remoting.Messaging;

    public class GalOrderStudents31075
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
        /// Источник финансирования
        /// </summary>
        [JsonProperty("finSourceCode")]
        public string FinSourceCode { get; set; } = string.Empty;

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
        /// Адрес общежития из которого переселют
        /// </summary>
        [JsonProperty("fromHostelAddr")]
        public string FromHostelAddr { get; set; }

        /// <summary>
        /// Номер общежития из которого переселют
        /// </summary>
        [JsonProperty("fromHostelNum")]
        public string FromHostelNum { get; set; }

        /// <summary>
        /// Блок в общежитии из которого переселют
        /// </summary>
        [JsonProperty("fromHostelBloc")]
        public string FromHostelBloc { get; set; }

        /// <summary>
        /// Номер комнаты из которого переселют
        /// </summary>
        [JsonProperty("fromHostelRoom")]
        public string FromHostelRoom { get; set; }

        /// <summary>
        /// Адрес общежития куда переселют
        /// </summary>
        [JsonProperty("toHostelAddr")]
        public string ToHostelAddr { get; set; }

        /// <summary>
        /// Номер общежития куда переселют
        /// </summary>
        [JsonProperty("toHostelNum")]
        public string ToHostelNum { get; set; }

        /// <summary>
        /// Блок в общежитии куда переселют
        /// </summary>
        [JsonProperty("toHostelBloc")]
        public string ToHostelBloc { get; set; }

        /// <summary>
        /// Номер комнаты куда переселют
        /// </summary>
        [JsonProperty("toHostelRoom")]
        public string ToHostelRoom { get; set; }

       /// <summary>
        /// Дата заселения
        /// </summary>
        [JsonProperty("dateFrom")]
        public string DateFrom { get; set; }

        /// <summary>
        /// Дата заселения
        /// </summary>
        [JsonProperty("dateEnd")]
        public string DateEnd { get; set; }

        /// <summary>
        /// Зав обшежитием
        /// </summary>
        [JsonProperty("managingHostel")]
        public string ManagingHostel { get; set; }
    }
}
