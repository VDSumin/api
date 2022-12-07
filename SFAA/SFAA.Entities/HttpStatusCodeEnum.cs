using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFAA.Entities
{
    /// <summary>
    /// Данный класс определяет статус кода для http ответа
    /// </summary>
    public enum HttpStatusCodeEnum : int
    {
        /// <summary>
        /// The global code.
        /// </summary>
        GlobalError = 400,
        GlobalSuccess = 200,
        Forbidden = 403,

        /// <summary>
        /// The fnpp good find teacher list.
        /// </summary>
        FnppGoodFindTeacherList = 201,
        FnppBadFindTeacherList = 401,

        StudentListFindGood = 202,
        BadNoNrecListFind = 402,
        BadListNotFound = 480
    }
}
