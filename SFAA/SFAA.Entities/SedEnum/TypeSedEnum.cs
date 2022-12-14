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
    public enum TypeSedEnum : int
    {
        //Смена ФИО
        [Description("000000007")]
        RPD30002 = 30002,

        //Восстановление
        [Description("000000018")]
        RPD30004 = 30004,

        //Восстановление на защиту ВКР
        [Description("000000019")]
        RPD300042 = 300042,

        //Перевод на обучение
        [Description("000000045")]
        RPD30005 = 30005,

        //Аттестация и перевод 
        [Description("000000006")]
        RPD300050 = 300050,

        //Аттестация и перевод 
        [Description("000000006")]
        RPD300051 = 300051,

        //Аттестация и перевод 
        [Description("000000006")]
        RPD300052 = 300052,

        //Аттестация и перевод 
        [Description("000000006")]
        RPD300053 = 300053,

        //Распределение по профилю
        [Description("000000043")]
        RPD30007 = 30007,

        //Отчиление
        [Description("000000042")]
        RPD30008 = 30008,

        //Гос соц стипендия
        [Description("000000023")]
        RPD30011 = 30011,

        //Предоставление академ отпуска
        [Description("000000034")]
        RPD30041 = 30041,

        //Выход из отпуска
        [Description("000000021")]
        RPD30042 = 30042,

        //Выход из академ отпуска
        [Description("000000044")]
        RPD30042_1 = 300421,

        //Предоставление последипломных каникул (групповой приказ)
        [Description("000000014")]
        RPD30045 = 30045,

        //Отчисление в связи с окончанием (групповой приказ)
        [Description("000000013")]
        RPD30081 = 30081,

        //Выпуск студентов
        [Description("000000013")]
        RPD30082 = 30082,

        //Отчисление в связи с окончанием университета, находящихся в последипломных каникулах (групповой приказ)
        [Description("000000016")]
        RPD30082_1 = 300821,

        //О предоставлении жилого помещения
        [Description("000000004")]
        RPD31074 = 31074,

        //Переселение - Изменение к договору
        [Description("000000003")]
        RPD31075 = 31075,

        //Расторжение договора о найме жилого помещения
        [Description("000000001")]
        RPD31076 = 31076,

        //Постановка на гос обеспечение
        [Description("000000047")]
        RPD30015 = 30015,

        //Перевод студентов форма обучения (основа обучения)
        [Description("000000046")]
        RPD30051 = 30051,

        //Продление академического отпуска
        [Description("000000036")]
        RPD30043 = 30043,

        //Отпуск по беременности и родам
        [Description("000000035")]
        RPD30044 = 30044,

        //Распределение по группам
        [Description("000000009")]
        RPD30006 = 30006,

        //Объединение групп
        [Description("000000009")]
        RPD30006_1 = 300061,

        //Перевод из группы в группу
        [Description("000000031")]
        RPD30006_2 = 300062,

        //Перевод на следующий курс
        [Description("000000048")]
        RPD30080 = 30080,

        //Приказ о зачисление студентов переводом
        [Description("000000025")]
        RPD30052 = 30052,

        //Перенос срока прохождения итоговый аттестаций
        [Description("000000012")]
        RPD31030 = 31030,

        //Продление сессии
        [Description("000000011")]
        RPD30030 = 30030,
    }


}
