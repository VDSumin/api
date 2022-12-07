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
    public enum FolderSedEnum : int
    {
        // TODO: Реализовать хранение данных значений в конфигурации
        //ОмГТУ по движению личного состава
        [Description("ДО-000271")]
        Folder1 = 0,

        //Колледж по движению личного состава
        [Description("ДО-000272")]
        Folder2 = 1,

        [Description("ДО-000179")]
        Folder3 = 2,

        //ОмГТУ по общежитию
        [Description("ДО-000279")]
        Folder4 = 3,

        //Колледж по общежитиям
        [Description("ДО-000278")]
        Folder5 = 4,

        //Аспирантура по личному составу
        [Description("ДО-000288")]
        Folder6 = 5,

        //Аспирантура по общежитию
        [Description("ДО-000289")]
        Folder7 = 5,

    }


}
