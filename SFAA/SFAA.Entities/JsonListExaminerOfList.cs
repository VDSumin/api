using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SFAA.Entities
{
    using System.Runtime.Remoting.Messaging;

    public class JsonListExaminerOfList
    {
        /// <summary>
        /// Nrec записи в таблице
        /// </summary>
        [JsonIgnore]
        public byte[] Nrec
        {
            get;
            set;
        }

        /// <summary>
        /// Nrec записи в таблице строковый
        /// </summary>
        [JsonProperty("nrec")]
        public string NrecString
        {
            get;
            set;
        }

        /// <summary>
        /// Nrec преподавателя
        /// </summary>
        [JsonIgnore]
        public byte[] NrecExaminer
        {
            get;
            set;
        }

        /// <summary>
        /// Nrec преподавателя строковый
        /// </summary>
        [JsonProperty("nrecExaminer")]
        public string NrecExaminerString
        {
            get;
            set;
        }


        /// <summary>
        /// ФИО преподавателя
        /// </summary>
        [JsonProperty("fioExaminer")]
        public string FioExaminer
        {
            get;
            set;
        }
        
    }
}
