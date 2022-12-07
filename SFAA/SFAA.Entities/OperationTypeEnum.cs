using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFAA.Entities
{
    /// <summary>
    /// Класс определяет тип операции
    /// </summary>
    public enum OperationTypeEnum : int
    {
        /// <summary>
        /// Операция ошибка
        /// </summary>
        Error = 0,

        /// <summary>
        /// Запрос всех ведомостей преподавателя
        /// </summary>
        FnppList = 1,

        /// <summary>
        /// Запрос содержимого ведомости
        /// </summary>
        GetListStruct = 2, 

        /// <summary>
        /// Запрос на обновление или вставку информации о рейтинге и часах посещаемости
        /// </summary>
        UpdateFieldRatingHours = 3,

        /// <summary>
        /// Запрос на обновление оценки и рейтинга студента
        /// </summary>
        UpdateMarkAndRating = 4,

        /// <summary>
        /// Запрос на получение каталога наименованеия оценко (отлично, хорошо и т.д.)
        /// </summary>
        GetCatalogMarks = 5,

        /// <summary>
        /// Плохой ключ api
        /// </summary>
        ErrorApiKey = 6,

        /// <summary>
        /// Запрос списка студентов
        /// </summary>
        GetAllStudents = 7,

        /// <summary>
        /// Запрос на удаление записи в таблице
        /// </summary>
        DeleteRecordFromTable = 8,

        /// <summary>
        /// Запрос на обновлении записи
        /// </summary>
        UpdateOneRecordIntoTable = 9,
            
        /// <summary>
        /// Запрос на получения списка тем курсовых работ по nrec ведомости
        /// </summary>
        GetKursThemes = 10,

        /// <summary>
        /// Запрос на модификацию тем курсовых работ (изменение, создание)
        /// </summary>
        ModifeKursTheme = 11,
        
        /// <summary>
        /// Запрос на получение списка напрвлений, привязанных к пользователю
        /// </summary>
        FnppExtraList = 12,

        /// <summary>
        /// Запрос на получение структуры направления
        /// </summary>
        GetExtraListStruct = 13,

        /// <summary>
        /// Запрос на обновеление ответственного и списка экзаменаторов
        /// </summary>
        UpdateExaminer = 14,

        /// <summary>
        /// Запрос на получение списка ППС зи галактики
        /// </summary>
        GetStuffForList = 15,

        /// <summary>
        /// Запрос на получение списа приказов
        /// </summary>
        GetOrders = 16,

        /// <summary>
        /// Запрос на получние списка направлений для студента
        /// </summary>
        GetExtraListForStudent = 17,

        /// <summary>
        /// Запрос на получение справочника дисциплин
        /// </summary>
        GetDisciplines = 18,

        /// <summary>
        /// Запрос на обновление номера приказа
        /// </summary>
        UpdateOrderNumb = 19,

        /// <summary>
        /// Запрос на получение всех договоров по общежитиям
        /// </summary>
        GetAllHostelContract = 20,

        /// <summary>
        /// Запрос для получения информации по всем госзакупкам
        /// </summary>
        GetAllGoszakupki = 21,

        // <summary>
        /// Запрос для получения информации по госзакупке
        /// </summary>
        GetGoszakupki = 22,

        // <summary>
        /// Запрос для получения информации об истории смены ФИО
        /// </summary>
        GetAllHistoryFioChange = 23,

        /// <summary>
        /// Запрос содержимого ведомости для диплома
        /// </summary>
        GetListDipStruct = 24,

        /// <summary>
        /// Запрос для обновления информации по ведомости для диплома
        /// </summary>
        UpdateDipMark = 25,

        /// <summary>
        /// Запрос для получения структуры рабочего плана
        /// </summary>
        GetWorkCurrStruct = 26,

        /// <summary>
        /// Запрос для блокировки пользователя
        /// </summary>
        BlockApiKey = 27,

        /// <summary>
        /// Запрещен доступ для данного api key
        /// </summary>
        AccessApiKey = 28,

        /// <summary>
        /// Запрос на создание пользователя
        /// </summary>
        CreateUser = 29,

        /// <summary>
        /// Запрос на список предприятий
        /// </summary>
        GetEntCat = 30,

        /// <summary>
        /// Запрос для получения зачетной книжки студента
        /// </summary>
        GetRecordBook = 31,

        /// <summary>
        /// Запрос для рабочих планов - проверка места нахождения дисциплины
        /// </summary>
        GetWorkCurrDisciplineType = 32,

        /// <summary>
        /// Запрос для обновления номера зачетной книжки
        /// </summary>
        UpdateRecordBook = 33,

        /// <summary>
        /// Запрос для получения информации по прохождению практики
        /// </summary>
        GetPracticeList = 34,

        /// <summary>
        /// Запрос для обновления  информации по прохождению практики
        /// </summary>
        UpdatePracticeList = 35,

        /// <summary>
        /// Запрос получения инормации о студентах группы из приема
        /// </summary>
        GetGroupStudents = 36,

        /// <summary>
        /// Запрос получения информации о предприятиях
        /// </summary>
        GetEnterpriseList = 37,

        /// <summary>
        /// Запрос для добавления/обновления информации о предприятиях
        /// </summary>
        UpdateEnterprise = 38,

        /// <summary>
        /// Запрос получения информации о преподавателе
        /// </summary>
        GetLectureInfo = 39,

        /// <summary>
        /// Запрос получения информации об учебном плане для заявления на общагу
        /// </summary>
        GetCurriculumInfo = 40
    }
}
