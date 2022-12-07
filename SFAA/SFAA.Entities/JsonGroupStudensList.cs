using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFAA.Entities
{
   public class JsonGroupStudentsList
    {
        /// <summary>
        /// ФИО студента
        /// </summary>
        [JsonProperty("fio")]
        public string Fio { get; set; } = string.Empty;
        
        /// <summary>
        /// FNPP студента из приема
        /// </summary>
        [JsonProperty("fnpp")]
        public int FNPP { get; set; } = 0;
    }
}
