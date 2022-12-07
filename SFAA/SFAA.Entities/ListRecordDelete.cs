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
    /// Сущность представляет собой набор свойств для одного студента
    /// </summary>
    public class ListRecordDelete
    {
        /// <summary>
        /// Nrec из таблицы строковый 
        /// </summary>
        public string Nrec { get; set; }

        /// <summary>
        /// Имя таблицы
        /// </summary>
        public string TableName { get; set; }

    }
}
