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
    public enum ViewSedEnum : int
    {
        [Description("Приказы по личному составу обучающихся ОмГТУ")]
        View1 = 0,
        [Description("Приказы по личному составу обучающихся ОмГТУ (иностранцы)")]
        View2 = 1,
        [Description("Приказы по личному составу обучающихся ОмГТУ (колледж)")]
        View3 = 2,
        [Description("Приказы о начислении стипендии и материальной помощи")]
        View4 = 3,
        [Description("Приказы по общежитиям ОмГТУ")]
        View5 = 4,
        [Description("Приказы по общежитиям ОмГТУ (иностранцы)")]
        View6 = 5,
        [Description("Приказы по общежитиям ОмГТУ (колледж)")]
        View7 = 6,
       
    }

    
}
