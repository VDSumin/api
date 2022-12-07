using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFAA.Entities
{
    /// <summary>
    /// Данный класс определяет статус ведомости
    /// </summary>
    public enum ListStatus : int
    {
        NEW = 0,
        OPEN = 1,
        CLOSE = 2
    }
}
