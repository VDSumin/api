using SFAA.Entities;
using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SFAA.DataOperation
{
    public class JsonParser
    {
        public object ParseJson(ActionData actionData)
        {

            switch (actionData.OperationType)
            {
                case OperationTypeEnum.FnppList:
                    return this.parseFnppFromJson(actionData);
                case OperationTypeEnum.GetListStruct:
                    return this.parseNrecListFromJson(actionData);
                case OperationTypeEnum.UpdateFieldRatingHours:
                    return this.parseUpdateListFromJson(actionData);
                case OperationTypeEnum.UpdateMarkAndRating:
                    return this.parseUpdateMarkAndRaingFromJson(actionData);
                case OperationTypeEnum.ModifeKursTheme:
                    return this.parseModifeKursTheme(actionData);
                case OperationTypeEnum.UpdateExaminer:
                    return this.parseUpdateExaminer(actionData);
                case OperationTypeEnum.UpdateDipMark:
                    return this.parseDipMark(actionData);
                case OperationTypeEnum.UpdateRecordBook:
                    return this.parseUpdateRecordBook(actionData);
                case OperationTypeEnum.UpdatePracticeList:
                    return this.parseUpdatePracticeFromJson(actionData);
                case OperationTypeEnum.UpdateEnterprise:
                    return this.parseUpdateEnterpriseFromJson(actionData);

            }

            return null;
        }

        private JsonStructList parseUpdateRecordBook(ActionData actionData)
        {
            var result = JsonConvert.DeserializeObject<JsonStructList>(actionData.JsonBody);

            if (!result.Student.Any())
            {
                Logger.Log.Error("От клиента пришел пустой запрос на обновление зачетных книжек");
                return null;
            }

            foreach (var t in result.Student)
            {
                try
                {
                    t.StudPersonNrec = DataOperation.Instance.StringHexToByteArray(t.StudPersonNrecString);
                }
                catch (Exception)
                {
                    t.StudPersonNrecString = DataOperation.Instance.GetNrecNullString;
                    t.StudPersonNrec = DataOperation.Instance.GetNrecNull;
                }

                try
                {
                    Encoding utf8 = Encoding.GetEncoding("UTF-8");
                    Encoding win1251 = Encoding.GetEncoding("Windows-1251");

                    byte[] utf8Bytes = win1251.GetBytes(t.RecordBookNumber);
                    byte[] win1251Bytes = Encoding.Convert(utf8, win1251, utf8Bytes);

                    t.RecordBookNumber = win1251.GetString(win1251Bytes);
                }
                catch (Exception e)
                {
                    //ignore
                }
            }

            return result;
        }

        private JsonStructListDip parseDipMark(ActionData actionData)
        {
            var resultMR = JsonConvert.DeserializeObject<JsonStructListDip>(actionData.JsonBody);

            if (resultMR.NrecString == null)
            {
                Logger.Log.Error("От клиента пришел пустой nrec ведомости для обновления данных по студентам");
                return null;
            }
            else
            {
                resultMR.NrecString.Trim();
                resultMR.Nrec = DataOperation.Instance.StringHexToByteArray(resultMR.NrecString);
                foreach (var t in resultMR.Student)
                {
                    t.MarkStudNrec = DataOperation.Instance.StringHexToByteArray(t.MarkStudNrecString);

                    t.MarkLinkNumberNrec = DataOperation.Instance.StringHexToByteArray(t.MarkLinkNumberNrecString);
                }
            }

            return resultMR;
        }

        /// <summary>
        /// Данный метод парсит json для обновления списка экзаменаторов
        /// </summary>
        /// <param name="actionData"></param>
        /// <returns></returns>
        private JsonStructList parseUpdateExaminer(ActionData actionData)
        {
            var result = JsonConvert.DeserializeObject<JsonStructList>(actionData.JsonBody);

            if (result.NrecString == null)
            {
                Logger.Log.Error("От клиента пришел пустой nrec ведомости для обновления данных по экзаменаторам");
                return null;
            }
            else
            {
                result.Nrec = DataOperation.Instance.StringHexToByteArray(result.NrecString.Trim());

                if (result.ExaminerNrecString == null)
                {
                    Logger.Log.Error("Ответственный по ведомости не определен");
                    return null;
                }

                result.ExaminerNrec = DataOperation.Instance.StringHexToByteArray(result.ExaminerNrecString.Trim());

                foreach (var t in result.ListExaminer)
                {
                    try
                    {
                        t.Nrec = DataOperation.Instance.StringHexToByteArray(t.NrecString);
                    }
                    catch (Exception)
                    {
                        t.NrecString = DataOperation.Instance.GetNrecNullString;
                        t.Nrec = DataOperation.Instance.GetNrecNull;
                    }

                    try
                    {
                        t.NrecExaminer = DataOperation.Instance.StringHexToByteArray(t.NrecExaminerString);
                    }
                    catch (Exception)
                    {
                        t.NrecExaminerString = DataOperation.Instance.GetNrecNullString;
                        t.NrecExaminer = DataOperation.Instance.GetNrecNull;
                    }

                }
            }

            return result;
        }

        /// <summary>
        /// Данный метод парсит json когда в нем хранится данные для обновления тем курсовых работ
        /// </summary>
        /// <param name="jsonBody"></param>
        /// <returns></returns>
        private JsonKursTheme parseModifeKursTheme(ActionData actionData)
        {
            var resultMKT = JsonConvert.DeserializeObject<JsonKursTheme>(actionData.JsonBody);

            if (resultMKT.NrecString.Length == 0)
            {
                Logger.Log.Error("От клиента пришел пустой nrec ведомости для обновления данных по курсовым работам");
                return null;
            }
            else
            {
                resultMKT.NrecString.Trim();
                resultMKT.Nrec = DataOperation.Instance.StringHexToByteArray(resultMKT.NrecString);
                foreach (var t in resultMKT.Student)
                {
                    try
                    {
                        t.MarkStudNrec = DataOperation.Instance.StringHexToByteArray(t.MarkStudNrecString);
                    }
                    catch (Exception)
                    {
                        return null;
                    }

                    try
                    {
                        t.StudPersonNrec = DataOperation.Instance.StringHexToByteArray(t.StudPersonNrecString);
                    }
                    catch (Exception)
                    {
                        t.StudPersonNrec = DataOperation.Instance.GetNrecNull;
                    }

                    try
                    {
                        t.KursThemeTeacherNrec = DataOperation.Instance.StringHexToByteArray(t.KursThemeTeacherNrecString);
                    }
                    catch (Exception)
                    {
                        t.KursThemeTeacherNrec = DataOperation.Instance.GetNrecNull;
                    }

                    try
                    {
                        t.DbDipNrec = DataOperation.Instance.StringHexToByteArray(t.DbDipNrecString);
                    }
                    catch (Exception)
                    {
                        t.DbDipNrec = DataOperation.Instance.GetNrecNull;
                    }

                    try
                    {
                        Encoding utf8 = Encoding.GetEncoding("UTF-8");
                        Encoding win1251 = Encoding.GetEncoding("Windows-1251");

                        byte[] utf8Bytes = win1251.GetBytes(t.KursTheme);
                        byte[] win1251Bytes = Encoding.Convert(utf8, win1251, utf8Bytes);

                        t.KursTheme = win1251.GetString(win1251Bytes);
                    }
                    catch (Exception e)
                    {
                        //ignore
                    }

                    if (t.KursTheme.Length > 255)
                    {
                        t.KursTheme = t.KursTheme.Substring(0, 255);
                    }

                }
            }

            return resultMKT;
        }

        /// <summary>
        /// Данный метод парсит json когда в нем хранится только fnpp - первый запрос клиента на поиск ведомостей
        /// </summary>
        /// <param name="actionData"></param>
        /// <returns></returns>
        private JsonFnpp parseFnppFromJson(ActionData actionData)
        {
            var resultFnpp = JsonConvert.DeserializeObject<JsonFnpp>(actionData.JsonBody);

            if (resultFnpp.Fnpp == null)
            {
                resultFnpp.Fnpp = string.Empty;
            }
            else
            {
                resultFnpp.Fnpp.Trim();
            }

            return resultFnpp;
        }

        /// <summary>
        /// Данный метод парсит json когда в нем хранится только nrec ведомости - второй запрос клиента на получение структуры ведомости
        /// </summary>
        /// <param name="actionData"></param>
        /// <returns></returns>
        private JsonStructList parseNrecListFromJson(ActionData actionData)
        {
            var resultSL = JsonConvert.DeserializeObject<JsonStructList>(actionData.JsonBody);

            if (resultSL.NrecString == null)
            {
                resultSL.NrecString = string.Empty;
            }
            else
            {
                resultSL.NrecString.Trim();
                resultSL.Nrec = DataOperation.Instance.StringHexToByteArray(resultSL.NrecString);
            }

            return resultSL;
        }

        /// <summary>
        /// Данный метод парсит json когда в нем хранятся данные для обновления рейтингов и часов по студентам
        /// </summary>
        /// <param name="actionData"></param>
        /// <returns></returns>
        private JsonStructList parseUpdateListFromJson(ActionData actionData)
        {
            var resultUp = JsonConvert.DeserializeObject<JsonStructList>(actionData.JsonBody);

            if (resultUp.NrecString == null)
            {
                Logger.Log.Error("От клиента пришел пустой nrec ведомости для обновления данных по студентам");
                return null;
            }
            else
            {
                resultUp.NrecString.Trim();
                resultUp.Nrec = DataOperation.Instance.StringHexToByteArray(resultUp.NrecString);
                foreach (var t in resultUp.Student)
                {
                    t.MarkStudNrec = DataOperation.Instance.StringHexToByteArray(t.MarkStudNrecString);
                }
            }

            return resultUp;
        }

        /// <summary>
        /// Данный метод парсит json когда в нем хранятся данные для обновления рейтингов и оценко по студентам
        /// </summary>
        /// <param name="actionData"></param>
        /// <returns></returns>
        private JsonStructList parseUpdateMarkAndRaingFromJson(ActionData actionData)
        {
            var resultMR = JsonConvert.DeserializeObject<JsonStructList>(actionData.JsonBody);

            if (resultMR.NrecString == null)
            {
                Logger.Log.Error("От клиента пришел пустой nrec ведомости для обновления данных по студентам");
                return null;
            }
            else
            {
                resultMR.NrecString.Trim();
                resultMR.Nrec = DataOperation.Instance.StringHexToByteArray(resultMR.NrecString);
                foreach (var t in resultMR.Student)
                {
                    t.MarkStudNrec = DataOperation.Instance.StringHexToByteArray(t.MarkStudNrecString);
                    t.MarkExaminerNrec = DataOperation.Instance.StringHexToByteArray(t.MarkExaminerNrecString);
                    t.MarkLinkNumberNrec = DataOperation.Instance.StringHexToByteArray(t.MarkLinkNumberNrecString);
                }
            }

            return resultMR;
        }

        /// <summary>
        /// Данный метод парсит json когда в нем хранятся данные для обновления инфы по прохождению практики
        /// </summary>
        /// <param name="actionData"></param>
        /// <returns></returns>
        private JsonPracticeList parseUpdatePracticeFromJson(ActionData actionData)
        {
            Logger.Log.Debug($"Вывод полученных данных: {actionData.JsonBody}");
            var resultMR = JsonConvert.DeserializeObject<JsonPracticeList>(actionData.JsonBody);
            Logger.Log.Debug($"Вывод объекта: {resultMR.Label}, {resultMR.ListNrecString}");
            var i = "";
            if (resultMR.ListNrecString == null)
            {
                Logger.Log.Error("От клиента пришел пустой nrec ведомости для обновления данных по практикам");
                return null;
            }
            else
            {
                resultMR.ListNrecString.Trim();
                resultMR.ListNrec = DataOperation.Instance.StringHexToByteArray(resultMR.ListNrecString);
                resultMR.ExaminerNrec = DataOperation.Instance.StringHexToByteArray(resultMR.ExaminerNrecString);
                resultMR.PersonNrec = DataOperation.Instance.StringHexToByteArray(resultMR.PersonNrecString);
                
                try
                {
                    Encoding utf8 = Encoding.GetEncoding("UTF-8");
                    Encoding win1251 = Encoding.GetEncoding("Windows-1251");

                    byte[] utf8Bytes = win1251.GetBytes(resultMR.Label);
                    byte[] win1251Bytes = Encoding.Convert(utf8, win1251, utf8Bytes);
                    i=win1251.GetString(win1251Bytes);
                    /*resultMR.Label = win1251.GetString(win1251Bytes);
                    
                    utf8Bytes = win1251.GetBytes(resultMR.Discipline);
                    win1251Bytes = Encoding.Convert(utf8, win1251, utf8Bytes);

                    resultMR.Discipline = win1251.GetString(win1251Bytes);*/
                }
                catch (Exception e)
                {
                    //ignore
                }
            }
            Logger.Log.Debug($"Вывод результатов запроса {resultMR.ListNrecString}, {resultMR.Label}, {i}");
            return resultMR;
        }


        private JsonEnterprises parseUpdateEnterpriseFromJson(ActionData actionData)
        {
            var resultMR = JsonConvert.DeserializeObject<JsonEnterprises>(actionData.JsonBody);

            if (resultMR.NrecString == null)
            {
                Logger.Log.Error("От клиента пришел пустой nrec ведомости для обновления данных по практикам");
                return null;
            }
            else
            {/*
                resultMR.ListNrecString.Trim();
                resultMR.ListNrec = DataOperation.Instance.StringHexToByteArray(resultMR.ListNrecString);
                resultMR.ExaminerNrec = DataOperation.Instance.StringHexToByteArray(resultMR.ExaminerNrecString);
                resultMR.PersonNrec = DataOperation.Instance.StringHexToByteArray(resultMR.PersonNrecString);*/
                /*try
                {
                    Encoding utf8 = Encoding.GetEncoding("UTF-8");
                    Encoding win1251 = Encoding.GetEncoding("Windows-1251");

                    byte[] utf8Bytes = win1251.GetBytes(resultMR.Label);
                    byte[] win1251Bytes = Encoding.Convert(utf8, win1251, utf8Bytes);

                    resultMR.Label = win1251.GetString(win1251Bytes);
                    
                    utf8Bytes = win1251.GetBytes(resultMR.Discipline);
                    win1251Bytes = Encoding.Convert(utf8, win1251, utf8Bytes);

                    resultMR.Discipline = win1251.GetString(win1251Bytes);
                }
                catch (Exception e)
                {
                    //ignore
                }*/
            }
            //Logger.Log.Debug($"Вывод результатов запроса {resultMR.ListNrecString}, {resultMR.Discipline}, {actionData.JsonBody}");
            return resultMR;
        }
    }
}
