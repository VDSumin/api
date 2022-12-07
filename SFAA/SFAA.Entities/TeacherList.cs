using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFAA.Entities
{
    /// <summary>
    /// Сущность предназначена для хранения информации о ведомости.
    /// </summary>
    public class TeacherList
    {
        /// <summary>
        /// Nrec ведомости
        /// </summary>
        public string Nrec
        {
            get; set;
        }

        /// <summary>
        /// Номер ведомости
        /// </summary>
        public string Numdoc
        {
            get; set;
        }

        /// <summary>
        /// Дисциплина ведомости
        /// </summary>
        public string Discipline
        {
            get; set;
        }

        /// <summary>
        /// Год ведомости
        /// </summary>
        public string Year
        {
            get; set;
        }

        /// <summary>
        /// Семестр ведомости
        /// </summary>
        public string Semester
        {
            get; set;
        }

        /// <summary>
        /// Стату ведомости
        /// </summary>
        public int Status
        {
            get; set;
        }

        /// <summary>
        /// Форма обучения группы из ведомости
        /// </summary>
        public int FormEdu
        {
            get; set;
        }

        /// <summary>
        /// Группа ведомости
        /// </summary>
        public string StudGroup
        {
            get; set;
        }

        /// <summary>
        /// Кафедра ведомости
        /// </summary>
        public string ListChair
        {
            get; set;
        }

        /// <summary>
        /// Факультет ведомости
        /// </summary>
        public string ListFacult
        {
            get; set;
        }

        /// <summary>
        /// Ответственный экзаменатор ведомости
        /// </summary>
        public string Examiner
        {
            get; set;
        }

    }
}
