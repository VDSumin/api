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
    public enum UlistTypeEnum : int
    {
        [Description("Зачёт")]
        Ladder = 1,
        [Description("Экзамен")]
        Exam = 2,
        [Description("Курсовая работа")]
        KursWork = 3,
        [Description("Курсовой проект")]
        KursProject = 4,
        [Description("Дипломная работа")]
        DipWork = 5,
        [Description("Дипломный проект")]
        DipProject = 6,
        [Description("Практики")]
        Practice = 9,
        [Description("Курсовая работа (внеочередная ведомость)")]
        ExtraKursWork = 53,
        [Description("Курсовой проект (внеочередная ведомость)")]
        ExtraKursProject = 54,
        [Description("Курсовая работа (направление)")]
        DirectKursWork = 103,
        [Description("Курсовой проект (направление)")]
        DirectKursProject = 104,

    }

    
}
