using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFAA.Entities;

namespace SFAA.DataOperation
{
    public class DataOperation
    {
        private static DataOperation _instance;

        public static DataOperation Instance => _instance ?? (_instance = new DataOperation());

        private static readonly string _nrecNull = "0x8000000000000000";

        private static readonly string _recordBookNrecString = "0x8001000000000003";

        public byte[] GetNrecNull => this.StringHexToByteArray(_nrecNull);

        public string GetNrecNullString => _nrecNull;

        /// <summary>
        /// Свойство значения родителя зачетной книжки строковое
        /// </summary>
        public string GetRecordBookNrecString => _recordBookNrecString;

        /// <summary>
        /// Свойство значения родителя зачетной книжки байтовое
        /// </summary>
        public byte[] GetRecordBookNrecByte => this.StringHexToByteArray(_recordBookNrecString);

        /// <summary>
        /// Данный метод конвертирует строку вида 0х800000000000015 в массив байтов
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        public byte[] StringHexToByteArray(string hex)
        {
            hex = hex.Trim();
            if (hex != String.Empty)
            {
                if (hex.Contains("0x"))
                {
                    hex = hex.Replace("0x", string.Empty).Trim();
                }

                return Enumerable.Range(0, hex.Length)
                    .Where(x => x % 2 == 0)
                    .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                    .ToArray();
            }
            else
            {
                hex = "8000000000000000";
                return Enumerable.Range(0, hex.Length)
                    .Where(x => x % 2 == 0)
                    .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                    .ToArray();
            }

        }

        /// <summary>
        /// Метод возвращает строковое предсталвение типа ведомости
        /// </summary>
        /// <param name="type">Тип из запроса</param>
        /// <returns></returns>
        public string GetUlistTypeDescriptio(int type)
        {
            var ulistTypeEnum = (UlistTypeEnum)Enum.GetValues(typeof(UlistTypeEnum)).GetValue(type);
            return ulistTypeEnum is UlistTypeEnum ? ulistTypeEnum.GetDescription() : "Тип неизвестен";
        }

        /// <summary>
        /// Данный метод возвращает преобразованные значения для родителей каталога оценок
        /// </summary>
        /// <returns></returns>
        public List<byte[]> GetParentCatalogsMarks()
        {
            var markExam = "0x8000000000000292";
            var markLadder = "0x8000000000000293";

            return new List<byte[]> { 
                this.StringHexToByteArray(markExam),
                this.StringHexToByteArray(markLadder)
                };
        }

        /// <summary>
        /// Метод преобразует byte[] в string
        /// </summary>
        /// <param name="byteValute"></param>
        /// <returns></returns>
        public string ByteToString(byte[] byteValue)
        {
            return string.Concat(
                "0x",
                BitConverter.ToString(byteValue).Replace("-", string.Empty));
        }

        /// <summary>
        /// Данный метод получает MD5 от строки
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public string CreateMD5(string input)
        {
            // Use input string to calculate MD5 hash
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }

                return sb.ToString();
            }
        }
    }
}
