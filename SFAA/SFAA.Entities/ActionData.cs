using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFAA.Entities
{
    public class ActionData
    {
        public ActionData()
        {

        }

        /// <summary>
        /// Ключ авторизации пользователя
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// Поток из запроса, который отправил пользователь
        /// </summary>
        public string RequestString { get; set; }

        /// <summary>
        /// Тип операции который приходит от сервера
        /// </summary>
        public OperationTypeEnum OperationType { get; set; }

        /// <summary>
        /// Тип запроса пользователя
        /// </summary>
        public RequestTypeEnum RequestType { get; set; }

        /// <summary>
        /// Json часть запроса от клиента
        /// </summary>
        public string JsonBody { get; set; }

        public object JsonDeserialize { get; set; }

        /// <summary>
        /// Nrec записи для удаления
        /// </summary>
        public ListRecordDelete RecordForDelete { get; set; }

        /// <summary>
        /// Nrec корневого объекта для поиска
        /// </summary>
        public string NrecOneRecord { get; set; }

        /// <summary>
        /// FNPP пользователя
        /// </summary>
        public string UserFnpp { get; set; }

        /// <summary>
        /// Произвольный параметр из запроса
        /// </summary>
        public string QueryParamNum1 { get; set; }

        /// <summary>
        /// Произвольный параметр из запроса
        /// </summary>
        public string QueryParamNum2 { get; set; }

        /// <summary>
        /// MD5 body
        /// </summary>
        public string RequestBodyMD5 { get; set; }

    }
}
