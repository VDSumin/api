using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFAA.Entities
{
    public class ServiceResponse
    {
        /// <summary>
        /// Код для ответа
        /// </summary>
        public int Code
        {
            get; set;
        }

        /// <summary>
        /// Строка успешности
        /// </summary>
        public bool Success
        {
            get; set;
        }

        /// <summary>
        /// Ошибка выполнения операции
        /// </summary>
        public string Error
        {
            get; set;

        }

        /// <summary>
        /// Байтовый массив для отправки, готовый ответ
        /// </summary>
        public byte[] BytesResponse
        {
            get; set;
        }

        /// <summary>
        /// Строковое представление ответа
        /// </summary>
        public string StringResponse
        {
            get; set;
        }
    }
}
