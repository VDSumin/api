using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using Newtonsoft.Json;
using SFAA.Entities;

namespace SFAA.DataOperation
{
   

    /// <summary>
    /// Данный класс формирует ответ для отправки клиенту
    /// </summary>
    public class CreateResponse
    {
        /// <summary>
        /// Данный метод формирует ответ в формате JSON с результатом - ошибка
        /// </summary>
        /// <returns><seealso cref="ServiceResponse"/></returns>
        public ServiceResponse GenerateErrorResponse(HttpStatusCodeEnum code, string errorString)
        {
            var json = JsonConvert.SerializeObject(new { success = false, error = errorString });

            var jsonByte = Encoding.UTF8.GetBytes(json);

            var codeStr = (int)code + " " + ((HttpStatusCode)400).ToString();

            var result = "HTTP/1.1 " + codeStr + "\nContent-Type: application/json; charset=UTF-8\nContent-Length:"
                         + jsonByte.Length.ToString() + "\n\n" + json.ToString();
            var buffer = Encoding.UTF8.GetBytes(result);

            var serviceResponse = new ServiceResponse { BytesResponse = buffer, StringResponse = result, Code = (int)code };

            return serviceResponse;
        }

        /// <summary>
        /// Данный метод формирует ответ в формате JSON с результатом - выполнено
        /// </summary>
        /// <param name="code">
        /// The code.
        /// </param>
        /// <param name="data">
        /// The data.
        /// </param>
        /// <returns>
        /// <seealso cref="ServiceResponse"/>
        /// </returns>
        public ServiceResponse GenerateGoodResponse(HttpStatusCodeEnum code, object data)
        {
            var json = JsonConvert.SerializeObject(data);

            var jsonByte = Encoding.UTF8.GetBytes(json);

            var codeStr = (int)code + " " + ((HttpStatusCode)200).ToString();

            var result = "HTTP/1.1 " + codeStr + "\nContent-Type: application/json; charset=UTF-8\r\nContent-Length:"
                         + jsonByte.Length.ToString() + "\n\n" + json.ToString();
            var buffer = Encoding.UTF8.GetBytes(result);

            var serviceResponse = new ServiceResponse { BytesResponse = buffer, StringResponse = result, Code = (int)code };

            return serviceResponse;
        }
    }
}
