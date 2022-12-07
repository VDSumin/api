using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFAA.Entities
{
    /// <summary>
    /// Данный класс определяет тип запроса пользователя
    /// </summary>
    public enum RequestTypeEnum : int
    {
        GET = 1,
        POST = 2,
        DELETE = 3,
        PUT = 4
    }
}
