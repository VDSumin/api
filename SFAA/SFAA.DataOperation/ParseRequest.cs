using System;
using System.Collections.Specialized;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using SFAA.Entities;
using System.Web;

namespace SFAA.DataOperation
{
    public class ParseRequest
    {
        private ActionData actionData;

        /// <summary>
        /// Инициализируем класс и помещаем строку, которая пришла от клиента
        /// </summary>
        /// <param name="request"></param>
        public ParseRequest(string request)
        {
            this.actionData = new ActionData()
                                  {
                                      RequestString = request,
                                      RequestBodyMD5 = DataOperation.Instance.CreateMD5(request)
                                  };
        }

        public void SetApiKey(string apikey)
        {
            this.actionData.ApiKey = apikey;
        }

        public string CheckApiKey(string request)
        {
            var strRegex = new Regex(@"(?<=up\.omgtu\.ru:\s)(.*?)(?=\s)");
            var strRegex2 = new Regex(@"(?<=mobile\.phone:\s)(.*?)(?=\s)");

            if (!string.IsNullOrEmpty(strRegex.Match(request).Value.Trim()))
            {
                return strRegex.Match(request).Value.Trim();
            }
            else
            {
                return strRegex2.Match(request).Value.Trim();
            }
        }

        /// <summary>
        /// Метод для разделения строки клиента для проведения операций
        /// </summary>
        /// <returns></returns>
        public ActionData GenerateActionData()
        {
            this.StartParse();
            return this.actionData;
        }

        /// <summary>
        /// Разделяем тело запроса на составляющие
        /// </summary>
        private void StartParse()
        {
            var splitString = this.actionData.RequestString.Split(' ');

            switch (splitString[0])
            {
                case "GET":
                    this.actionData.RequestType = RequestTypeEnum.GET;
                    this.GetOperationTypeFromGet(splitString[1]);
                    break;
                case "POST":
                    this.actionData.RequestType = RequestTypeEnum.POST;
                    this.GetOperationTypeFromPost(splitString[1]);
                    this.GetJsonBody();
                    break;
                case "DELETE":
                    this.actionData.RequestType = RequestTypeEnum.DELETE;
                    this.GetOperationTypeFromDelete(splitString[1]);
                    break;
                case "PUT":
                    this.actionData.RequestType = RequestTypeEnum.PUT;
                    this.GetOperationTypeFromPut(splitString[1]);
                    break;
                default:
                    break;
            }

        }

        //TODO: Убрать или доделать /updateListDate
        private void GetOperationTypeFromPut(string tempSplit)
        {
            try
            {
                string[] split = { };
                try
                {
                    split = tempSplit.Split('?');
                }
                catch (Exception)
                {
                    split[0] = tempSplit;
                }
                var parse = new NameValueCollection();
                if (split.Any())
                {
                    try
                    {
                        parse = HttpUtility.ParseQueryString(split[1]);
                    }
                    catch (Exception)
                    {
                        //ignore
                    }
                }

                switch (split[0])
                {
                    case "/updateOrderNumb":
                        this.actionData.OperationType = OperationTypeEnum.UpdateOrderNumb;
                        actionData.NrecOneRecord = parse["nrec"];
                        actionData.QueryParamNum1 = parse["numb"];
                        break;
                    case "/modifeKursTheme":
                        this.actionData.OperationType = OperationTypeEnum.ModifeKursTheme;
                        this.GetJsonBody();
                        break;
                    case "/updateDipMark":
                        this.actionData.OperationType = OperationTypeEnum.UpdateDipMark;
                        this.GetJsonBody();
                        break;
                    case "/updateRecordBook":
                        this.actionData.OperationType = OperationTypeEnum.UpdateRecordBook;
                        this.GetJsonBody();
                        break;
                    case "/blockApiKey":
                        this.actionData.OperationType = OperationTypeEnum.BlockApiKey;
                        actionData.QueryParamNum1 = parse["key"];
                        break;
                    case "/createUser":
                        this.actionData.OperationType = OperationTypeEnum.CreateUser;
                        actionData.QueryParamNum1 = parse["login"];
                        break;
                }
            }
            catch (Exception exception)
            {
                Logger.Log.Error($"При определении типа операции из get параметра произошла ошибка. Не удалось определить тип операции. Ошибка = {exception}");
                this.actionData.OperationType = OperationTypeEnum.Error;
            }

        }

        /// <summary>
        /// Получаем json из запроса
        /// </summary>
        private void GetJsonBody()
        {
            var strRegex = new Regex(@"(?<=\{)(.*?)(?=\}$)");
            this.actionData.JsonBody = string.Concat('{', strRegex.Match(this.actionData.RequestString).Value.Trim(), '}');
            this.actionData.JsonDeserialize = JsonConvert.DeserializeObject(this.actionData.JsonBody);
        }

        /// <summary>
        /// Данный метод определяет тип операции из post запроса
        /// </summary>
        /// <param name="tempSplit"></param>
        private void GetOperationTypeFromPost(string tempSplit)
        {
            var split = tempSplit.Split('?');
            switch (split[0])
            {
                case "/fnppList":
                    this.actionData.OperationType = OperationTypeEnum.FnppList;
                    break;
                case "/getListStruct":
                    this.actionData.OperationType = OperationTypeEnum.GetListStruct;
                    break;
                case "/updateFieldRatingHours":
                    this.actionData.OperationType = OperationTypeEnum.UpdateFieldRatingHours;
                    break;
                case "/updateMarkAndRating":
                    this.actionData.OperationType = OperationTypeEnum.UpdateMarkAndRating;
                    break;
                case "/updateExaminer":
                    this.actionData.OperationType = OperationTypeEnum.UpdateExaminer;
                    break;
                case "/updatePracticeList":
                    this.actionData.OperationType = OperationTypeEnum.UpdatePracticeList;
                    break;
                case "/updateEnterprise":
                    this.actionData.OperationType = OperationTypeEnum.UpdateEnterprise;
                    break;
                default:
                    this.actionData.OperationType = OperationTypeEnum.Error;
                    break;

            }

        }

        /// <summary>
        /// Данный метод определяет тип операции из get запроса
        /// </summary>
        /// <param name="tempSplit">Исходная строка с параметрами</param>
        private void GetOperationTypeFromGet(string tempSplit)
        {
            try
            {
                string[] split = {};
                try
                {
                    split = tempSplit.Split('?');
                }
                catch (Exception)
                {
                    split[0] = tempSplit;
                }
                var parse = new NameValueCollection();
                if (split.Any())
                {
                    try
                    {
                        parse = HttpUtility.ParseQueryString(split[1]);
                    }
                    catch (Exception)
                    {
                        //ignore
                    }
                }
                
                switch (split[0])
                {
                    case "/fnppExtraList":
                        this.actionData.OperationType = OperationTypeEnum.FnppExtraList;
                        actionData.UserFnpp = parse["fnpp"].Trim();
                        break;
                    case "/getExtraListStruct":
                        this.actionData.OperationType = OperationTypeEnum.GetExtraListStruct;
                        actionData.NrecOneRecord = parse["nrec"].Trim();
                        break;
                    case "/getCatalogMarks":
                        this.actionData.OperationType = OperationTypeEnum.GetCatalogMarks;
                        break;
                    case "/getDisciplines":
                        this.actionData.OperationType = OperationTypeEnum.GetDisciplines;
                        break;
                    case "/getOrders":
                        this.actionData.OperationType = OperationTypeEnum.GetOrders;
                        break;
                    case "/getAllStudents":
                        this.actionData.OperationType = OperationTypeEnum.GetAllStudents;
                        try
                        {
                            actionData.QueryParamNum1 = parse["id"];
                        }
                        catch (Exception )
                        {
                            actionData.QueryParamNum1 = string.Empty;
                        }
                        break;
                    case "/getKursTheme":
                        this.actionData.OperationType = OperationTypeEnum.GetKursThemes;
                        actionData.NrecOneRecord = parse["nrec"];
                        break;
                    case "/getStuffForList":
                        this.actionData.OperationType = OperationTypeEnum.GetStuffForList;
                        actionData.NrecOneRecord = parse["nrec"];
                        actionData.QueryParamNum1 = parse["all"];
                        break;
                    case "/getExtraListForStudent":
                        this.actionData.OperationType = OperationTypeEnum.GetExtraListForStudent;
                        actionData.NrecOneRecord = parse["nrec"];
                        break;
                    case "/getAllHostelContract":
                        this.actionData.OperationType = OperationTypeEnum.GetAllHostelContract;
                        break;
                    case "/getAllGoszakupki":
                        this.actionData.OperationType = OperationTypeEnum.GetAllGoszakupki;
                        break;
                    case "/getGoszakupki":
                        this.actionData.OperationType = OperationTypeEnum.GetGoszakupki;
                        actionData.QueryParamNum1 = parse["code"];
                        break;
                    case "/getAllHistoryFioChange":
                        this.actionData.OperationType = OperationTypeEnum.GetAllHistoryFioChange;
                        break;
                    case "/getListDipStruct":
                        this.actionData.OperationType = OperationTypeEnum.GetListDipStruct;
                        actionData.NrecOneRecord = parse["nrec"];
                        break;
                    case "/getWorkCurrStruct":
                        this.actionData.OperationType = OperationTypeEnum.GetWorkCurrStruct;
                        actionData.NrecOneRecord = parse["nrec"].Trim();
                        break;
                    case "/getEntCat":
                        this.actionData.OperationType = OperationTypeEnum.GetEntCat;
                        try
                        {
                            actionData.QueryParamNum1 = parse["hash"];
                        }
                        catch (Exception)
                        {
                            actionData.QueryParamNum1 = string.Empty;
                        }
                        break;
                    case "/getRecordBook":
                        this.actionData.OperationType = OperationTypeEnum.GetRecordBook;
                        try
                        {
                            actionData.NrecOneRecord = parse["nrec"].Trim();
                        }
                        catch (Exception)
                        {
                            actionData.NrecOneRecord = string.Empty;
                        }
                        try
                        {
                            actionData.QueryParamNum2 = parse["site"].Trim();
                        }
                        catch (Exception)
                        {
                            actionData.QueryParamNum2 = string.Empty;
                        }
                        break;
                    case "/getWorkCurrDisciplineType":
                        this.actionData.OperationType = OperationTypeEnum.GetWorkCurrDisciplineType;
                        try
                        {
                            actionData.NrecOneRecord = parse["nrec"].Trim();
                        }
                        catch (Exception)
                        {
                            actionData.NrecOneRecord = string.Empty;
                        }
                        break;
                    case "/getPracticeList":
                        this.actionData.OperationType = OperationTypeEnum.GetPracticeList;
                        try
                        {
                            actionData.NrecOneRecord = parse["nrec"].Trim();
                        }
                        catch (Exception)
                        {
                            actionData.NrecOneRecord = string.Empty;
                        }
                        break;
                    case "/getGroupStudents":
                        this.actionData.OperationType = OperationTypeEnum.GetGroupStudents;
                        try
                        {
                            actionData.QueryParamNum1 = parse["groupName"].Trim();
                        }
                        catch (Exception)
                        {

                            actionData.QueryParamNum1 = string.Empty;
                        }
                        break;
                    case "/getEnterprises":
                        this.actionData.OperationType = OperationTypeEnum.GetEnterpriseList;
                        try
                        {
                            actionData.QueryParamNum1 = parse["hash"];
                        }
                        catch (Exception)
                        {
                            actionData.QueryParamNum1 = string.Empty;
                        }
                        break;
                    case "/getLectureInfo":
                        this.actionData.OperationType = OperationTypeEnum.GetLectureInfo;
                        try
                        {
                            actionData.QueryParamNum1 = parse["fnpp"];
                        }
                        catch (Exception)
                        {
                            actionData.QueryParamNum1 = string.Empty;
                        }
                        break;
                    case "/getCurriculumInfoForHostel":
                        this.actionData.OperationType = OperationTypeEnum.GetCurriculumInfo;
                        try
                        {
                            actionData.NrecOneRecord = parse["nrec"].Trim();
                        }
                        catch (Exception)
                        {
                            actionData.NrecOneRecord = string.Empty;
                        }
                        break;
                }
            }
            catch (Exception exception)
            {
                Logger.Log.Error($"При определении типа операции из get параметра произошла ошибка. Не удалось определить тип операции. Ошибка = {exception}");
                this.actionData.OperationType = OperationTypeEnum.Error;
            }

        }

        /// <summary>
        /// Данный метод определяет тип операции из delete запроса
        /// </summary>
        /// <param name="tempSplit"></param>
        private void GetOperationTypeFromDelete(string tempSplit)
        {
            try
            {
                var split = tempSplit.Split('?');
                switch (split[0])
                {
                    case "/delreclistexam":
                        this.actionData.OperationType = OperationTypeEnum.DeleteRecordFromTable;
                        var listDeleteRecord = new ListRecordDelete();
                        var parse = HttpUtility.ParseQueryString(split[1]);
                        listDeleteRecord.Nrec = parse["nrec"];
                        listDeleteRecord.TableName = "T$U_LIST_EXAMINER";
                        actionData.RecordForDelete = listDeleteRecord;
                        break;
                }
            }
            catch (Exception exception)
            {
                Logger.Log.Error($"При определении типа операции из delete параметра произошла ошибка. Не удалось определить тип операции. Ошибка = {exception}");
                this.actionData.OperationType = OperationTypeEnum.Error;
            }
        }
    }
}
