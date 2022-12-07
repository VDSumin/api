using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SFAA.Entities
{
    using System.Runtime.Remoting.Messaging;

    public class JsonFnpp
    {
        /// <summary>
        /// Fnpp преподавателя
        /// </summary>
        [JsonProperty("fnpp")]
        public string Fnpp
        {
            get;
            set;
        }
    }
}
