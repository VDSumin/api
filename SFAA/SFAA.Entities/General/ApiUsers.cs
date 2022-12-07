using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net.Util;

namespace SFAA.Entities
{
    /// <summary>
    /// 
    /// </summary>
    public class ApiUsers
    {
        public int Id { get; set; } = 0;

        public int GalStatus { get; set; } = 0;

        public string ApiKey { get; set; } = string.Empty;

    }
}
