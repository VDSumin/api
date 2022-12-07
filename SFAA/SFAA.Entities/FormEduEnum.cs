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
    public enum FormEduEnum : int
    {
        [Description("очное обучение")]
        Internal = 0,
        [Description("заочное обучение")]
        Extramural = 1,
        [Description("очно-заочное обучение")]
        Evening = 2
    }

    
}
