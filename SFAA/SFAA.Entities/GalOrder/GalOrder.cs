using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SFAA.Entities
{
    using System.Runtime.Remoting.Messaging;

    public class GalOrder
    {
        /// <summary>
        /// Nrec TitleDoc
        /// </summary>
        [JsonIgnore]
        public byte[] Nrec
        {
            get;
            set;
        }

        /// <summary>
        /// Nrec TitleDoc строковый
        /// </summary>
        [JsonProperty("nrec")]
        public string NrecString { get; set; } = "0x8000000000000000";

        /// <summary>
        /// Nrec TitleDoc Int64
        /// </summary>
        [JsonProperty("nrecInt64")]
        public Int64 NrecInt64 { get; set; } = 0;

        /// <summary>
        /// Когда последний раз редактировался
        /// </summary>
        [JsonIgnore]
        public string LastDateEditTitle { get; set; } = string.Empty;

        /// <summary>
        /// Когда последний раз редактировался
        /// </summary>
        [JsonIgnore]
        public string LastDateEditPart { get; set; } = string.Empty;

        /// <summary>
        /// Когда последний раз редактировался
        /// </summary>
        [JsonIgnore]
        public string LastDateEditCont { get; set; } = string.Empty;
        /// <summary>
        /// Когда последний раз редактировался
        /// </summary>
        [JsonProperty("lastDateEdit")]
        public string LastDateEdit { get; set; } = string.Empty;

        /// <summary>
        /// Номер приказа
        /// </summary>
        [JsonProperty("docNmb")]
        public string DocNmb { get; set; } = string.Empty;

        /// <summary>
        /// Дата приказа
        /// </summary>
        [JsonProperty("docDate")]
        public string DocDate { get; set; } = string.Empty;

        /// <summary>
        /// Год приказа
        /// </summary>
        [JsonProperty("docYear")]
        public string DocYear { get; set; } = string.Empty;

        /// <summary>
        /// Факультет приказа
        /// </summary>
        [JsonProperty("facultyOrder")]
        public string FacultyOrder { get; set; } = string.Empty;

        /// <summary>
        /// Факультет приказа аббревиатура
        /// </summary>
        [JsonProperty("facultyOrderAbbr")]
        public string FacultyOrderAbbr { get; set; } = string.Empty;

        /// <summary>
        /// Краткое содержание
        /// </summary>
        [JsonProperty("docText")]
        public string DocText { get; set; } = string.Empty;

        /// <summary>
        /// РПД с доп кодом
        /// </summary>
        [JsonProperty("rpd")]
        public string Rpd { get; set; } = string.Empty;

        /// <summary>
        /// РПД имя
        /// </summary>
        [JsonProperty("rpdName")]
        public string RpdName { get; set; } = string.Empty;

        /// <summary>
        /// Автор приказа
        /// </summary>
        [JsonProperty("authorDoc")]
        public string AuthorDoc { get; set; } = string.Empty;

        /// <summary>
        /// Основание приказа
        /// </summary>
        [JsonProperty("basisOfOrder")]
        public string BasisOfOrder { get; set; } = string.Empty;

        /// <summary>
        /// Тип документа в сэд
        /// </summary>
        public string TypeSed { get; set; } = string.Empty;

        /// <summary>
        /// Вид документа в сэд
        /// </summary>
        public string ViewSed { get; set; } = string.Empty;

        /// <summary>
        /// Папка документа в сэд
        /// </summary>
        public string FolderSed { get; set; } = string.Empty;

        /// <summary>
        /// Перечень студентов
        /// </summary>
        [JsonProperty("students")]
        public object Students { get; set; } = new object();

        /// <summary>
        /// Подписанты приказа
        /// </summary>
        [JsonProperty("signature")]
        public List<GalOrderSignature> Signature { get; set; } = new List<GalOrderSignature>();
    }
}
