using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace SFAA.Entities
{
    /// <summary>
    /// Класс определяет тип ведомостей
    /// </summary>
    public enum MarkTypeEnum : int
    {
        [Description("Окончательная")]
        Final = 1,
        [Description("Перевод")]
        Transfer = 2,
        [Description("Переаттестация")]
        Recertification = 3,
        [Description("Текущая")]
        Current = 0,
    }

    
}
