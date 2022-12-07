using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFAA.ClientServerOperation;
using SFAA.Entities;
using System.Net;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using SFAA.DataOperation;
using SFAA.DBAdapter;

namespace SFAA.Business
{
    public class MainBusiness
    {
        private static MainBusiness _instance;

        private Listener listener;

        private JsonParser jsonParser;

        private CreateResponse createResponse;

        private DBAdapterOperationPriem dbPriemAdapter;

        private DBAdapterOperationGalaxy dbGalaxyAdapter;

        private DBAdapterLocalDB _dbAdapterLocalDb;

        private DataOperation.DataOperation _dataOperation;

        private IMemoryCache _cache;

        /// <summary>
        /// Gets the instance.
        /// </summary>
        public static MainBusiness Instance => _instance ?? (_instance = new MainBusiness());

        /// <summary>
        /// Данный метод выполняет инициализацию объектов
        /// </summary>
        public void TryInialize()
        {
            _cache = new MemoryCache(new MemoryCacheOptions());
            this.jsonParser = new JsonParser();
            this.createResponse = new CreateResponse();
            this.dbPriemAdapter = new DBAdapterOperationPriem();
            this.dbGalaxyAdapter = new DBAdapterOperationGalaxy();
            this._dbAdapterLocalDb = new DBAdapterLocalDB();
            this._dataOperation = new DataOperation.DataOperation();
        }


        /// <summary>
        /// Данный метод запускает прием клиентов
        /// </summary>
        public void StartListener()
        {
            Logger.Log.Info($"Запущен слушатель в {DateTime.Now.ToString()}");
            this.listener = new Listener(this.DoAction);
        }

        private ServiceResponse DoAction(ActionData actionData)
        {
            var response = this.ExecuteOperationsByActionData(actionData);
            return response;
        }

        /// <summary>
        /// Данный метод выбирает какую операцию нужно выполнить в зависимости от запроса клиента
        /// </summary>
        /// <param name="actionData"></param>
        /// <returns></returns>
        private ServiceResponse ExecuteOperationsByActionData(ActionData actionData)
        {
            var serviceResponse = new ServiceResponse();

            try
            {
                switch (actionData.OperationType)
                {
                    case OperationTypeEnum.FnppList:
                        serviceResponse = this.ExecuteAllListByFnppOperation(actionData);
                        break;
                    case OperationTypeEnum.FnppExtraList:
                        serviceResponse = this.ExecuteExtraListByFnppOperation(actionData);
                        break;
                    case OperationTypeEnum.GetListStruct:
                        serviceResponse = this.ExecuteGetStructureOfListByNrecList(actionData);
                        break;
                    case OperationTypeEnum.GetExtraListStruct:
                        serviceResponse = this.ExecuteGetExtraListStructureOfListByNrecList(actionData);
                        break;
                    case OperationTypeEnum.UpdateFieldRatingHours:
                        serviceResponse = this.ExecuteUpdateFieldRatingHoursByJsonData(actionData);
                        break;
                    case OperationTypeEnum.UpdateMarkAndRating:
                        serviceResponse = this.ExecuteUpdateMarkAndRatinByJsonData(actionData);
                        break;
                    case OperationTypeEnum.UpdateExaminer:
                        serviceResponse = this.ExecuteUpdateExaminerByJsonData(actionData);
                        break;
                    case OperationTypeEnum.GetCatalogMarks:
                        serviceResponse = this.ExecuteGetCatalogsMarks(actionData);
                        break;
                    case OperationTypeEnum.GetDisciplines:
                        serviceResponse = this.ExecuteGetDisciplines(actionData);
                        break;
                    case OperationTypeEnum.Error:
                        serviceResponse = this.ExecuteErrorOperation(HttpStatusCodeEnum.GlobalError, "Фатальная ошибка");
                        break;
                    case OperationTypeEnum.ErrorApiKey:
                        serviceResponse = this.ExecuteErrorOperation(HttpStatusCodeEnum.GlobalError, "Нельзя использовать указанный Api Key");
                        break;
                    case OperationTypeEnum.AccessApiKey:
                        serviceResponse = this.ExecuteErrorOperation(HttpStatusCodeEnum.GlobalError, "Данный Api Key заблокирован");
                        break;
                    case OperationTypeEnum.GetAllStudents:
                        serviceResponse = this.ExecuteGetAllStudentsByJsonData(actionData);
                        break;
                    case OperationTypeEnum.DeleteRecordFromTable:
                        serviceResponse = this.ExecuteDeleteRecordFromTable(actionData);
                        break;
                    case OperationTypeEnum.GetKursThemes:
                        serviceResponse = this.ExecuteGetKursThemes(actionData);
                        break;
                    case OperationTypeEnum.ModifeKursTheme:
                        serviceResponse = this.ExecuteModifeKursTheme(actionData);
                        break;
                    case OperationTypeEnum.GetStuffForList:
                        serviceResponse = this.ExecuteGetStuffForList(actionData);
                        break;
                    case OperationTypeEnum.GetExtraListForStudent:
                        serviceResponse = this.ExecuteGetExtraListForStudent(actionData);
                        break;
                    case OperationTypeEnum.GetOrders:
                        serviceResponse = this.ExecuteGetOrders(actionData);
                        break;
                    case OperationTypeEnum.GetAllHostelContract:
                        serviceResponse = this.ExecuteGetAllHostelContract(actionData);
                        break;
                    case OperationTypeEnum.GetAllGoszakupki:
                        serviceResponse = this.ExecuteGetGoszakupki(actionData);
                        break;
                    case OperationTypeEnum.GetGoszakupki:
                        serviceResponse = this.ExecuteGetGoszakupki(actionData);
                        break;
                    case OperationTypeEnum.GetAllHistoryFioChange:
                        serviceResponse = this.ExecuteGetAllHistoryFioChange(actionData);
                        break;
                    case OperationTypeEnum.GetListDipStruct:
                        serviceResponse = this.ExecuteGetListDipStructByNrecList(actionData);
                        break;
                    case OperationTypeEnum.UpdateDipMark:
                        serviceResponse = this.ExecuteUpdateDipMarkByJsonData(actionData);
                        break;
                    case OperationTypeEnum.GetWorkCurrStruct:
                        serviceResponse = this.ExecuteGetWorkCurrStruct(actionData);
                        break;
                    case OperationTypeEnum.BlockApiKey:
                        serviceResponse = this.ExecuteBlockApiKey(actionData);
                        break;
                    case OperationTypeEnum.CreateUser:
                        serviceResponse = this.ExecuteCreateUser(actionData);
                        break;
                    case OperationTypeEnum.GetEntCat:
                        serviceResponse = this.ExecuteGetEntCat(actionData);
                        break;
                    case OperationTypeEnum.GetRecordBook:
                        serviceResponse = this.ExecuteGetRecordBook(actionData);
                        break;
                    case OperationTypeEnum.GetWorkCurrDisciplineType:
                        serviceResponse = this.ExecuteGetWorkCurrDisciplineType(actionData);
                        break;
                    case OperationTypeEnum.UpdateRecordBook:
                        serviceResponse = this.ExecuteUpdateRecordBook(actionData);
                        break;
                    case OperationTypeEnum.GetPracticeList:
                        serviceResponse = this.ExecuteGetPracticeList(actionData);
                        break;
                    case OperationTypeEnum.UpdatePracticeList:
                        serviceResponse = this.ExecuteUpdatePracticeListByJsonData(actionData);
                        break;
                    case OperationTypeEnum.GetGroupStudents:
                        serviceResponse = this.ExecuteGetGroupStudents(actionData);
                        break;
                    case OperationTypeEnum.GetEnterpriseList:
                        serviceResponse = this.ExecuteGetEnterpriseList();
                        break;
                    case OperationTypeEnum.UpdateEnterprise:
                        serviceResponse = this.ExecuteUpdateEnterprise(actionData);
                        break;
                    case OperationTypeEnum.GetLectureInfo:
                        serviceResponse = this.ExecuteGetLectureInfo(actionData);
                        break;
                    case OperationTypeEnum.GetCurriculumInfo:
                        serviceResponse = this.ExecuteGetCurriculumInfo(actionData);
                        break;

                        
                }
            }
            catch (Exception exception)
            {
                Logger.Log.Error($"Ошибка на этапе старта обработки. Ошибка = {exception}");
                serviceResponse = this.ExecuteErrorOperation(HttpStatusCodeEnum.GlobalError, "Ошибка на этапе старта обработки");
            }

            return serviceResponse;
        }


        /// <summary>
        /// Данный метод выполняет получение информации об учебном плане для заявления на общагу
        /// </summary>
        /// <param name="actionData"></param>
        /// <returns></returns>
        private ServiceResponse ExecuteGetCurriculumInfo(ActionData actionData)
        {
            var serviceResponse = new ServiceResponse();
            var structList = new List<JsonCurriculumInfo>();

            try
            {
                if (actionData.NrecOneRecord != null && actionData.NrecOneRecord.Length > 14)
                {
                    structList = this.dbGalaxyAdapter.GetCurriculumInfoForHostel(actionData.NrecOneRecord);
                    serviceResponse = this.ExecuteGoodOperation(HttpStatusCodeEnum.GlobalSuccess, structList);
                }
                else
                {
                    serviceResponse = this.ExecuteGoodOperation(HttpStatusCodeEnum.GlobalError, "Ошибка при получении информации о плане");
                }
            }
            catch (Exception e)
            {
                Logger.Log.Debug($"Ошибка при получении информации о плане. Ошибка {e}");
                serviceResponse = this.ExecuteGoodOperation(HttpStatusCodeEnum.GlobalError, "Ошибка при получении информации о плане");
            }
            return serviceResponse;
        }

        /// <summary>
        /// Данный метод выполняет получение информации о студентах из группы
        /// </summary>
        /// <param name="actionData">Название группы</param>
        /// <returns></returns>
        private ServiceResponse ExecuteGetGroupStudents(ActionData actionData)
        {
            ServiceResponse serviceResponse = new ServiceResponse();
            List<JsonGroupStudentsList> structList = new List<JsonGroupStudentsList>();

            try
            {
                if (actionData.QueryParamNum1 != null)
                {
                    structList = this.dbGalaxyAdapter.GetGroupStudentsList(actionData.QueryParamNum1);
                    serviceResponse = this.ExecuteGoodOperation(HttpStatusCodeEnum.GlobalSuccess, structList);
                }
                else
                {
                    serviceResponse = this.ExecuteErrorOperation(HttpStatusCodeEnum.GlobalError, "Ошибка при получении списка групп");
                }
            }
            catch (Exception e)
            {

                Logger.Log.Debug($"Ошибка при получени списка групп. Ошибка {e}");
                serviceResponse = this.ExecuteErrorOperation(HttpStatusCodeEnum.GlobalError, "Ошибка при получении списка групп");
            }

            return serviceResponse;
        }

        /// <summary>
        /// Данный метод выполняет получение информации о прохождении практики 
        /// </summary>
        /// <param name="actionData"></param>
        /// <returns></returns>
        private ServiceResponse ExecuteGetPracticeList(ActionData actionData)
        {
            var serviceResponse = new ServiceResponse();
            var structList = new List<JsonPracticeList>();

            try
            {
                if (actionData.NrecOneRecord != null && actionData.NrecOneRecord.Length > 14)
                {
                    structList = this.dbGalaxyAdapter.GetPracticeList(actionData.NrecOneRecord);
                    serviceResponse = this.ExecuteGoodOperation(HttpStatusCodeEnum.GlobalSuccess, structList);

                }
                else
                {
                    serviceResponse = this.ExecuteGoodOperation(HttpStatusCodeEnum.GlobalError, "Ошибка при получении информации о прохождении практики");
                }
            }
            catch (Exception e)
            {
                Logger.Log.Debug($"Ошибка при получении информации о прохождении практики . Ошибка {e}");
                serviceResponse = this.ExecuteGoodOperation(HttpStatusCodeEnum.GlobalError, "Ошибка при получении информации о прохождении практики");
            }
            return serviceResponse;
        }

        /// <summary>
        /// Данный метод выполняет обновлене зачетной книжки студента
        /// </summary>
        /// <param name="actionData"></param>
        /// <returns></returns>
        private ServiceResponse ExecuteUpdateRecordBook(ActionData actionData)
        {
            var serviceResponse = new ServiceResponse();
            var structList = new JsonStructList();

            try
            {
                structList = (JsonStructList)this.jsonParser.ParseJson(actionData);
            }
            catch (Exception e)
            {
                Logger.Log.Debug($"Ошибка при парсинге JSON для изъятия информации для обновления данных по зачетной книжке. Ошибка {e}");
                serviceResponse = this.ExecuteErrorOperation(HttpStatusCodeEnum.BadNoNrecListFind, "Не удалось сделать десерилизацию объекта");
                return serviceResponse;
            }


            if (!structList.Student.Any())
            {
                Logger.Log.Info($"От клиента пришел пустой запрос на обновление зачетной книжки");
                serviceResponse = this.ExecuteErrorOperation(
                    HttpStatusCodeEnum.GlobalError,
                    "Не удалось определить данные по зачетной книжки для обновления");
                return serviceResponse;
            }
            try
            {
                var resultRecordBookUpdate = this.dbGalaxyAdapter.UpdateRecordBookIntoDb(structList);
                if (!resultRecordBookUpdate)
                {
                    serviceResponse = this.ExecuteErrorOperation(HttpStatusCodeEnum.GlobalError, "Не удалось обновить зачетные книжки. Дальнейшее сохранение невозможно");
                }
                else
                {
                    serviceResponse = this.ExecuteGoodOperation(
                        HttpStatusCodeEnum.GlobalSuccess,
                        "Зачетные книжки успешно обновлены");

                }

            }
            catch (Exception e)
            {
                Logger.Log.Debug($"Ошибка сохранении данных в галактику. Ошибка {e}");
                serviceResponse = this.ExecuteErrorOperation(HttpStatusCodeEnum.GlobalError, "Ошибки при сохранении номеров зачетных книжек.");
            }

            return serviceResponse;
        }

        private ServiceResponse ExecuteGetWorkCurrDisciplineType(ActionData actionData)
        {
            var serviceResponse = new ServiceResponse();
            var structList = new List<ListWorkCurrDisciplineType>();

            try
            {
                if (actionData.NrecOneRecord != null && actionData.NrecOneRecord.Length > 14)
                {
                    structList = this.dbGalaxyAdapter.GetGetWorkCurrDisciplineTypeFromDb(actionData.NrecOneRecord);
                    serviceResponse = this.ExecuteGoodOperation(HttpStatusCodeEnum.GlobalSuccess, structList);

                }
                else
                {
                    serviceResponse = this.ExecuteGoodOperation(HttpStatusCodeEnum.GlobalError, "Ошибка при получении структуры для рабочего плана");
                }
            }
            catch (Exception e)
            {
                Logger.Log.Debug($"Ошибка при получении структуры для рабочего плана. Ошибка {e}");
                serviceResponse = this.ExecuteGoodOperation(HttpStatusCodeEnum.GlobalError, "Ошибка при получении структуры для рабочего плана");
            }





            return serviceResponse;
        }

        /// <summary>
        /// Данный метод получает зачетную книжку по студенту
        /// </summary>
        /// <param name="actionData"></param>
        /// <returns></returns>
        private ServiceResponse ExecuteGetRecordBook(ActionData actionData)
        {
            var serviceResponse = new ServiceResponse();
            var structList = new List<ListOneRecordFromRecordBook>();

            try
            {
                if (actionData.NrecOneRecord != null)
                {

                    try
                    {
                        if (_cache.TryGetValue(actionData.RequestBodyMD5, out structList))
                        {
                            Logger.Log.Info($"Результат по запросу отправлен из кэша.");
                            serviceResponse = this.ExecuteGoodOperation(HttpStatusCodeEnum.GlobalSuccess, structList);
                            return serviceResponse;
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Log.Info($"Ошибка обращения к кэшу памяти. Ошибка {e}");
                    }

                    Int64 tempNrec;
                    if (Int64.TryParse(actionData.NrecOneRecord, out tempNrec))
                    {
                        structList = this.dbGalaxyAdapter.GetRecordBookFromDb(tempNrec, actionData.QueryParamNum2);
                        _cache.Set(actionData.RequestBodyMD5, structList,
                            new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(10)));
                        Logger.Log.Info("Результат выборки по зачетной книжке внесен в кэш");
                        serviceResponse = this.ExecuteGoodOperation(HttpStatusCodeEnum.GlobalSuccess, structList);
                    }
                    else
                    {
                        serviceResponse = this.ExecuteGoodOperation(HttpStatusCodeEnum.GlobalError, "Не правильный id студента");
                    }

                }
                else
                {
                    serviceResponse = this.ExecuteGoodOperation(HttpStatusCodeEnum.GlobalError, "Не правильный id студента");
                }
            }
            catch (Exception e)
            {
                Logger.Log.Error($"Ошибка при формировании зачетной книжки. Ошибка {e}");
                serviceResponse = this.ExecuteGoodOperation(HttpStatusCodeEnum.GlobalError, "Не правильный id студента");
            }

            return serviceResponse;
        }

        /// <summary>
        /// Даннй метод возвращает список предприятий для практики
        /// </summary>
        /// <param name="actionData"></param>
        /// <returns></returns>
        private ServiceResponse ExecuteGetEntCat(ActionData actionData)
        {
            var serviceResponse = new ServiceResponse();
            var structList = new List<JsonEntCat>();
            try
            {
                var jsonMemory = new JsonEnterpriseHash()
                {
                    Hash = string.Empty,
                    Enterprises = structList
                };
                if (_cache.TryGetValue(actionData.RequestBodyMD5, out jsonMemory))
                {
                    Logger.Log.Info($"Результат по запросу мест практики отправлен из кэша.");
                    serviceResponse = this.ExecuteGoodOperation(HttpStatusCodeEnum.GlobalSuccess, jsonMemory);
                    return serviceResponse;
                }
            }
            catch (Exception e)
            {
                Logger.Log.Info($"Ошибка обращения к кэшу памяти. Ошибка {e}");
            }
            try
            {

                structList = this.dbGalaxyAdapter.GetEntCat(structList);
                _cache.Set(actionData.RequestBodyMD5, structList,
                            new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(10)));
                Logger.Log.Info("Результат выборки предприятий внесен в кэш");
                if (actionData.QueryParamNum1 != null)
                {
                    var hash = _dataOperation.CreateMD5(structList.ToString());
                    serviceResponse = this.ExecuteGoodOperation(HttpStatusCodeEnum.GlobalSuccess, hash);
                }
                else
                {
                    serviceResponse = this.ExecuteGoodOperation(HttpStatusCodeEnum.GlobalSuccess, structList);
                }
            }
            catch (Exception e)
            {
                Logger.Log.Debug($"Ошибка при формировании списка предприятий для практики. Ошибка {e}");
                serviceResponse = this.ExecuteErrorOperation(HttpStatusCodeEnum.GlobalError, "Не сформировать список предприятий для практики");
            }
            return serviceResponse;
        }

        /// <summary>
        /// Данный метод выполняет создание пользователя
        /// </summary>
        /// <param name="actionData"></param>
        /// <returns></returns>
        private ServiceResponse ExecuteCreateUser(ActionData actionData)
        {
            var serviceResponse = new ServiceResponse();
            if (GlobalSettings.Instance.AuthDataGalLogin != "administrator")
            {
                Logger.Log.Debug($"Только администратор может выполнить данное действие.");
                serviceResponse = this.ExecuteErrorOperation(HttpStatusCodeEnum.GlobalError, "Только администратор может выполнить данное действие.");
            }
            else
            {
                if (actionData.QueryParamNum1.Length < 2)
                {
                    Logger.Log.Debug($"Пришел пустой запрос");
                    serviceResponse =
                        this.ExecuteErrorOperation(HttpStatusCodeEnum.GlobalError, "Пришел пустой запрос");
                    return serviceResponse;
                }

                try
                {
                    var result = _dbAdapterLocalDb.CreateUserByLoginIntoDb(actionData.QueryParamNum1);
                    if (!string.Equals(result, string.Empty))
                    {
                        Logger.Log.Debug($"Пользователь создан");
                        serviceResponse =
                            this.ExecuteGoodOperation(HttpStatusCodeEnum.GlobalSuccess, result);

                    }
                    else
                    {
                        Logger.Log.Debug($"Такой пользователь существует");
                        serviceResponse =
                            this.ExecuteErrorOperation(HttpStatusCodeEnum.GlobalError, "Такой пользователь существует");
                    }
                }
                catch (Exception e)
                {
                    Logger.Log.Debug($"Ошибка при создании пользователя. Ошибка {e}");
                    serviceResponse =
                        this.ExecuteErrorOperation(HttpStatusCodeEnum.GlobalError, "Ошибка при создании пользователя.");
                }

            }

            return serviceResponse;
        }

        /// <summary>
        /// Данный метод выполняет блокировку api key пользователя
        /// </summary>
        /// <param name="actionData"></param>
        /// <returns></returns>
        private ServiceResponse ExecuteBlockApiKey(ActionData actionData)
        {
            var serviceResponse = new ServiceResponse();
            if (GlobalSettings.Instance.AuthDataGalLogin != "administrator")
            {
                Logger.Log.Debug($"Только администратор может выполнить данное действие.");
                serviceResponse = this.ExecuteErrorOperation(HttpStatusCodeEnum.GlobalError, "Только администратор может выполнить данное действие.");
            }
            else
            {
                var result = _dbAdapterLocalDb.BlockApiKey(actionData.QueryParamNum1);
                if (result)
                {
                    Logger.Log.Debug($"Ключ заблокирован.");
                    serviceResponse =
                        this.ExecuteGoodOperation(HttpStatusCodeEnum.GlobalSuccess, "Ключ заблокирован.");

                }
                else
                {
                    Logger.Log.Debug($"Ключ незаблокирован.");
                    serviceResponse =
                        this.ExecuteErrorOperation(HttpStatusCodeEnum.GlobalError, "Ключ незаблокирован.");
                }
            }

            return serviceResponse;
        }

        /// <summary>
        /// Данный метод выполняет операции по получению структуры рабочего плана
        /// </summary>
        /// <param name="actionData"></param>
        /// <returns></returns>
        private ServiceResponse ExecuteGetWorkCurrStruct(ActionData actionData)
        {
            var serviceResponse = new ServiceResponse();
            var structList = new JsonWorkCurrStruct();

            try
            {
                if (actionData.NrecOneRecord.Length == 0)
                {
                    Logger.Log.Info($"От клиента пришел пустой Nrec рабочего плана");
                    serviceResponse = this.ExecuteErrorOperation(
                        HttpStatusCodeEnum.BadNoNrecListFind,
                        "Не удалось определить Nrec рабочего плана");
                    return serviceResponse;
                }
                else
                {
                    structList.NrecString = actionData.NrecOneRecord;
                    structList.Nrec = _dataOperation.StringHexToByteArray(structList.NrecString);
                }
            }
            catch (Exception e)
            {
                Logger.Log.Debug($"Ошибка при парсинге JSON для изъятия nrec рабочего плана. Ошибка {e}");
                serviceResponse = this.ExecuteErrorOperation(HttpStatusCodeEnum.BadNoNrecListFind, "Не удалось определить Nrec рабочего плана");
                return serviceResponse;
            }

            //Выполняется проверка, что данный клиент может выполнять операции с ведомостями
            //if (!_dbAdapterLocalDb.CheckAccessToWorkWithList(actionData.ApiKey, structList.NrecString))
            //{
            //    serviceResponse = this.ExecuteErrorOperation(HttpStatusCodeEnum.Forbidden, "Вам запрещены операции с данным элементом!");
            //    return serviceResponse;
            //}

            try
            {
                structList = this.dbGalaxyAdapter.GetGetWorkCurrStruct(structList);

                serviceResponse = this.ExecuteGoodOperation(HttpStatusCodeEnum.StudentListFindGood, structList);


            }
            catch (Exception e)
            {
                Logger.Log.Debug($"Ошибка при поиске ведомости в базе. Ошибка {e}");
                serviceResponse = this.ExecuteErrorOperation(HttpStatusCodeEnum.BadListNotFound, "Не удалось найти ведомость");
            }

            return serviceResponse;
        }

        /// <summary>
        /// Обновление инорфмации по ведомости для диплома
        /// </summary>
        /// <param name="actionData"></param>
        /// <returns></returns>
        private ServiceResponse ExecuteUpdateDipMarkByJsonData(ActionData actionData)
        {
            var serviceResponse = new ServiceResponse();
            var structList = new JsonStructListDip();

            try
            {
                structList = (JsonStructListDip)this.jsonParser.ParseJson(actionData);
            }
            catch (Exception e)
            {
                Logger.Log.Debug($"Ошибка при парсинге JSON для изъятия информации для обновления по ведомости для диплома. Ошибка {e}");
                serviceResponse = this.ExecuteErrorOperation(HttpStatusCodeEnum.BadNoNrecListFind, "Не удалось сделать десерилизацию объекта");
                return serviceResponse;
            }

            //Выполняется проверка, что данный клиент может выполнять операции с ведомостями
            if (!_dbAdapterLocalDb.CheckAccessToWorkWithList(actionData.ApiKey, structList.NrecString))
            {
                serviceResponse = this.ExecuteErrorOperation(HttpStatusCodeEnum.Forbidden, "Вам запрещены операции с данным элементом!");
                return serviceResponse;
            }

            if (structList.NrecString.Length == 0)
            {
                Logger.Log.Info($"От клиента пришел пустой Nrec ведомости");
                serviceResponse = this.ExecuteErrorOperation(
                    HttpStatusCodeEnum.BadNoNrecListFind,
                    "Не удалось определить Nrec ведомости");
                return serviceResponse;
            }

            try
            {
                var resultUpdate = this.dbGalaxyAdapter.UpdateDipMarkByNrecTableMark(structList);
                if (!resultUpdate)
                {
                    serviceResponse = this.ExecuteErrorOperation(HttpStatusCodeEnum.GlobalError, "Не удалось обновить информацию по ведомости для диплома. Дальнейшее сохранение невозможно");
                }
                else
                {
                    serviceResponse = this.ExecuteGoodOperation(HttpStatusCodeEnum.GlobalSuccess, "Информацию по ведомости для диплома успешно обновлена");

                }

            }
            catch (Exception e)
            {
                Logger.Log.Debug($"Ошибка сохранении данных в галактику. Ошибка {e}");
                serviceResponse = this.ExecuteErrorOperation(HttpStatusCodeEnum.GlobalError, "Ошибки при сохранении  информации по ведомости для диплома.");
            }

            return serviceResponse;
        }

        /// <summary>
        /// Данный метод получает структуру ведомости по диплому
        /// </summary>
        /// <param name="actionData"></param>
        /// <returns></returns>
        private ServiceResponse ExecuteGetListDipStructByNrecList(ActionData actionData)
        {
            var serviceResponse = new ServiceResponse();
            var structList = new JsonStructListDip();

            try
            {
                if (actionData.NrecOneRecord.Length == 0)
                {
                    Logger.Log.Info($"От клиента пришел пустой Nrec ведомости");
                    serviceResponse = this.ExecuteErrorOperation(
                        HttpStatusCodeEnum.BadNoNrecListFind,
                        "Не удалось определить Nrec ведомости");
                    return serviceResponse;
                }
                else
                {
                    structList.NrecString = actionData.NrecOneRecord;
                    structList.Nrec = _dataOperation.StringHexToByteArray(structList.NrecString);
                }
            }
            catch (Exception e)
            {
                Logger.Log.Debug($"Ошибка при парсинге JSON для изъятия nrec ведомости, по которой ищются студенты. Ошибка {e}");
                serviceResponse = this.ExecuteErrorOperation(HttpStatusCodeEnum.BadNoNrecListFind, "Не удалось определить Nrec ведомости");
                return serviceResponse;
            }

            //Выполняется проверка, что данный клиент может выполнять операции с ведомостями
            if (!_dbAdapterLocalDb.CheckAccessToWorkWithList(actionData.ApiKey, structList.NrecString))
            {
                serviceResponse = this.ExecuteErrorOperation(HttpStatusCodeEnum.Forbidden, "Вам запрещены операции с данным элементом!");
                return serviceResponse;
            }

            try
            {
                structList = this.dbGalaxyAdapter.GetListDipByNrec(structList);
                if (structList.NumDoc != string.Empty)
                {
                    structList.Student = this.dbGalaxyAdapter.GetStudentFromListDipByNrecList(structList.Nrec);
                    structList.ListExaminer = this.dbGalaxyAdapter.GetListExaminerFromListByNrecList(structList.Nrec);

                    serviceResponse = this.ExecuteGoodOperation(HttpStatusCodeEnum.StudentListFindGood, structList);
                }
                else
                {
                    serviceResponse = this.ExecuteErrorOperation(HttpStatusCodeEnum.BadListNotFound, "Не удалось найти ведомость");
                }
            }
            catch (Exception e)
            {
                Logger.Log.Debug($"Ошибка при поиске ведомости в базе. Ошибка {e}");
                serviceResponse = this.ExecuteErrorOperation(HttpStatusCodeEnum.BadListNotFound, "Не удалось найти ведомость");
            }

            return serviceResponse;
        }

        /// <summary>
        /// Данный метод получае информации об истории смены ФИО
        /// </summary>
        /// <param name="actionData"></param>
        /// <returns></returns>
        private ServiceResponse ExecuteGetAllHistoryFioChange(ActionData actionData)
        {
            var serviceResponse = new ServiceResponse();
            var structList = new List<JsonHistoryFioChange>();

            try
            {
                structList = this.dbGalaxyAdapter.GetAllHistoryFioChangeFromDb();
                if (structList.Any())
                {
                    serviceResponse = this.ExecuteGoodOperation(HttpStatusCodeEnum.GlobalSuccess, structList);
                }
                else
                {
                    serviceResponse = this.ExecuteErrorOperation(HttpStatusCodeEnum.GlobalError, "Не удалось собрать история изменения ФИО");
                }
            }
            catch (Exception e)
            {
                Logger.Log.Debug($"Ошибка при сборе истории ФИО. Ошибка {e}");
                serviceResponse = this.ExecuteErrorOperation(HttpStatusCodeEnum.BadListNotFound, "Не удалось собрать история изменения ФИО");
            }

            return serviceResponse;
        }

        /// <summary>
        /// Данный метод получает информаци о госзакупках
        /// </summary>
        /// <param name="actionData"></param>
        /// <returns></returns>
        private ServiceResponse ExecuteGetGoszakupki(ActionData actionData)
        {
            var serviceResponse = new ServiceResponse();
            var structList = new GoszakupkiJson();
            var code = string.Empty;
            if (actionData.QueryParamNum1 != null && actionData.QueryParamNum1.Length > 1)
            {
                code = actionData.QueryParamNum1;
            }
            try
            {
                ProcessStartInfo start = new ProcessStartInfo();

                start.FileName = GlobalSettings.Instance.PythonPath;

                start.Arguments = string.Format("{0} {1} {2}", GlobalSettings.Instance.GoszakupkiPythonScript, GlobalSettings.Instance.GoszakupkiJsonFile, code);
                start.UseShellExecute = false;
                start.RedirectStandardOutput = true;
                string result = "";
                using (Process process = Process.Start(start))
                {
                    using (StreamReader reader = process.StandardOutput)
                    {
                        result = reader.ReadToEnd();
                    }
                }

                result = result.Replace("\r\n", "");
                result = result.Replace("b'", "");
                result = result.Replace("}'", "");

                string contents = File.ReadAllText(GlobalSettings.Instance.GoszakupkiJsonFile, Encoding.GetEncoding("windows-1251"));
                contents = contents.Replace("b'", "");
                contents = contents.Replace("\\xd0", "");
                contents = contents.Replace("\\xb2", "");

                structList = JsonConvert.DeserializeObject<GoszakupkiJson>(contents);

                if (structList.Elements.Any())
                {
                    structList.Hash = DataOperation.DataOperation.Instance.CreateMD5(structList.Elements.ToString());
                    serviceResponse = this.ExecuteGoodOperation(HttpStatusCodeEnum.GlobalSuccess, structList);
                }
                else
                {
                    serviceResponse = this.ExecuteErrorOperation(HttpStatusCodeEnum.GlobalError, "Не удалось обработать госзакупки");
                }
            }
            catch (Exception e)
            {
                Logger.Log.Debug($"Не удалось обработать госзакупки. Ошибка {e}");
                serviceResponse = this.ExecuteErrorOperation(HttpStatusCodeEnum.BadListNotFound, "Не удалось обработать госзакупки");
            }

            return serviceResponse;
        }

        /// <summary>
        /// Данный метод получает все договора по общежитиям
        /// </summary>
        /// <param name="actionData"></param>
        /// <returns></returns>
        private ServiceResponse ExecuteGetAllHostelContract(ActionData actionData)
        {
            var serviceResponse = new ServiceResponse();
            var structList = new List<ListHostelContract>();

            try
            {
                structList = this.dbPriemAdapter.GetAllHostelContractFromDb();
                if (structList.Any())
                {
                    serviceResponse = this.ExecuteGoodOperation(HttpStatusCodeEnum.GlobalSuccess, structList);
                }
                else
                {
                    serviceResponse = this.ExecuteErrorOperation(HttpStatusCodeEnum.GlobalError, "Не удалось собрать список договоров");
                }
            }
            catch (Exception e)
            {
                Logger.Log.Debug($"Ошибка при сборе списка договоров. Ошибка {e}");
                serviceResponse = this.ExecuteErrorOperation(HttpStatusCodeEnum.BadListNotFound, "Не удалось собрать список договоров");
            }

            return serviceResponse;
        }

        /// <summary>
        /// Данный метод возвращает справочник дисциплин
        /// </summary>
        /// <param name="actionData"></param>
        /// <returns></returns>
        private ServiceResponse ExecuteGetDisciplines(ActionData actionData)
        {
            var serviceResponse = new ServiceResponse();
            var structList = new List<ListDiscipline>();

            try
            {
                structList = this.dbGalaxyAdapter.GetDisciplinesFromDb();
                if (structList.Any())
                {
                    var hash = _dataOperation.CreateMD5(structList.ToString());
                    var jsonReturn = new JsonDisciplines()
                    {
                        Hash = hash,
                        Disciplines = structList
                    };
                    serviceResponse = this.ExecuteGoodOperation(HttpStatusCodeEnum.GlobalSuccess, jsonReturn);
                }
                else
                {
                    serviceResponse = this.ExecuteErrorOperation(HttpStatusCodeEnum.BadListNotFound, "Не удалось собрать каталог дисциплин");
                }
            }
            catch (Exception e)
            {
                Logger.Log.Debug($"Ошибка при поиске дисциплин в каталоге. Ошибка {e}");
                serviceResponse = this.ExecuteErrorOperation(HttpStatusCodeEnum.BadListNotFound, "Не удалось собрать каталог дисциплин");
            }

            return serviceResponse;
        }

        /// <summary>
        /// Данный метод получает список активных приказов для согласования в СЭД
        /// </summary>
        /// <param name="actionData"></param>
        /// <returns></returns>
        private ServiceResponse ExecuteGetOrders(ActionData actionData)
        {
            var serviceResponse = new ServiceResponse();
            var orders = new List<GalOrder>();

            try
            {
                var rpd30002 = dbGalaxyAdapter.GetOrdersRpd30002FromDb();
                orders.AddRange(rpd30002);
            }
            catch (Exception e)
            {
                Logger.Log.Debug($"На РПД 30002 возникла ошибка. Ошибка: {e}");
            }

            try
            {
                var rpd30004 = dbGalaxyAdapter.GetOrdersRpd30004FromDb();
                orders.AddRange(rpd30004);
            }
            catch (Exception e)
            {
                Logger.Log.Debug($"На РПД 30004 возникла ошибка. Ошибка: {e}");
            }

            try
            {
                //Перевод специальность
                var rpd30005 = dbGalaxyAdapter.GetOrdersRpd30005FromDb();
                orders.AddRange(rpd30005);
            }
            catch (Exception e)
            {
                Logger.Log.Debug($"На РПД 30005 возникла ошибка. Ошибка: {e}");
            }

            try
            {
                //30006 Распределение студентов по группам
                var rpd30006 = dbGalaxyAdapter.GetOrdersRpd30006FromDb();
                orders.AddRange(rpd30006);
            }
            catch (Exception e)
            {
                Logger.Log.Debug($"На РПД 30006 возникла ошибка. Ошибка: {e}");
            }

            try
            {
                //Профильный приказ
                var rpd30007 = dbGalaxyAdapter.GetOrdersRpd30007FromDb();
                orders.AddRange(rpd30007);
            }
            catch (Exception e)
            {
                Logger.Log.Debug($"На РПД 30007 возникла ошибка. Ошибка: {e}");
            }

            try
            {
                ///Проверка приказов РПД 30008
                var rpd30008 = dbGalaxyAdapter.GetOrdersRpd30008FromDb();
                orders.AddRange(rpd30008);
            }
            catch (Exception e)
            {
                Logger.Log.Debug($"На РПД 30008 возникла ошибка. Ошибка: {e}");
            }

            try
            {
                var rpd30011 = dbGalaxyAdapter.GetOrdersRpd30011FromDb();
                orders.AddRange(rpd30011);
            }
            catch (Exception e)
            {
                Logger.Log.Debug($"На РПД 30011 возникла ошибка. Ошибка: {e}");
            }

            try
            {
                //Пересеоение
                var rpd30015 = dbGalaxyAdapter.GetOrdersRpd30015FromDb();
                orders.AddRange(rpd30015);
            }
            catch (Exception e)
            {
                Logger.Log.Debug($"На РПД 30015 возникла ошибка. Ошибка: {e}");
            }

            try
            {
                //Продление сессии
                var rpd30030 = dbGalaxyAdapter.GetOrdersRpd30030FromDb();
                orders.AddRange(rpd30030);
            }
            catch (Exception e)
            {
                Logger.Log.Debug($"На РПД 30015 возникла ошибка. Ошибка: {e}");
            }

            try
            {
                var rpd30041 = dbGalaxyAdapter.GetOrdersRpd30041FromDb();
                orders.AddRange(rpd30041);
            }
            catch (Exception e)
            {
                Logger.Log.Debug($"На РПД 30041 возникла ошибка. Ошибка: {e}");
            }

            try
            {
                var rpd30042 = dbGalaxyAdapter.GetOrdersRpd30042FromDb();
                orders.AddRange(rpd30042);
            }
            catch (Exception e)
            {
                Logger.Log.Debug($"На РПД 30042 возникла ошибка. Ошибка: {e}");
            }

            try
            {
                var rpd30043 = dbGalaxyAdapter.GetOrdersRpd30043FromDb();
                orders.AddRange(rpd30043);
            }
            catch (Exception e)
            {
                Logger.Log.Debug($"На РПД 30043 возникла ошибка. Ошибка: {e}");
            }

            try
            {
                //30051 Перевод студентов (основа обучения)
                var rpd30044 = dbGalaxyAdapter.GetOrdersRpd30044FromDb();
                orders.AddRange(rpd30044);
            }
            catch (Exception e)
            {
                Logger.Log.Debug($"На РПД 30044 возникла ошибка. Ошибка: {e}");
            }

            try
            {
                var rpd30045 = dbGalaxyAdapter.GetOrdersRpd30045FromDb();
                orders.AddRange(rpd30045);
            }
            catch (Exception e)
            {
                Logger.Log.Debug($"На РПД 30045 возникла ошибка. Ошибка: {e}");
            }

            try
            {
                //30051 Перевод студентов (основа обучения)
                var rpd30051 = dbGalaxyAdapter.GetOrdersRpd30051FromDb();
                orders.AddRange(rpd30051);
            }
            catch (Exception e)
            {
                Logger.Log.Debug($"На РПД 30051 возникла ошибка. Ошибка: {e}");
            }

            try
            {
                //30052 Приказ о зачислении студента переводом
                var rpd30052 = dbGalaxyAdapter.GetOrdersRpd30052FromDb();
                orders.AddRange(rpd30052);
            }
            catch (Exception e)
            {
                Logger.Log.Debug($"На РПД 30052 возникла ошибка. Ошибка: {e}");
            }

            try
            {
                //30080 Перевод на следующий курс
                var rpd30080 = dbGalaxyAdapter.GetOrdersRpd30080FromDb();
                orders.AddRange(rpd30080);
            }
            catch (Exception e)
            {
                Logger.Log.Debug($"На РПД 30080 возникла ошибка. Ошибка: {e}");
            }

            try
            {
                //31030 Перенос срока прохождения итоговых аттестационных испытаний
                var rpd31030 = dbGalaxyAdapter.GetOrdersRpd31030FromDb();
                orders.AddRange(rpd31030);
            }
            catch (Exception e)
            {
                Logger.Log.Debug($"На РПД 31030 возникла ошибка. Ошибка: {e}");
            }

            /*try
            {
                //Заселение в общежитие
                var rpd31074 = dbGalaxyAdapter.GetOrdersRpd31074FromDb();
                orders.AddRange(rpd31074);
            }
            catch (Exception e)
            {
                Logger.Log.Debug($"На РПД 31074 возникла ошибка. Ошибка: {e}");
            }*/

            try
            {
                //Пересеоение
                var rpd31075 = dbGalaxyAdapter.GetOrdersRpd31075FromDb();
                orders.AddRange(rpd31075);
            }
            catch (Exception e)
            {
                Logger.Log.Debug($"На РПД 31075 возникла ошибка. Ошибка: {e}");
            }

            try
            {
                //Выселение из общежития
                var rpd31076 = dbGalaxyAdapter.GetOrdersRpd31076FromDb();
                orders.AddRange(rpd31076);
            }
            catch (Exception e)
            {
                Logger.Log.Debug($"На РПД 31076 возникла ошибка. Ошибка: {e}");
            }

            try
            {
                var rpd30081 = dbGalaxyAdapter.GetOrdersRpd30081FromDb();
                orders.AddRange(rpd30081);
            }
            catch (Exception e)
            {
                Logger.Log.Debug($"На РПД 30081 возникла ошибка. Ошибка: {e}");
            }

            try
            {
                var rpd30082 = dbGalaxyAdapter.GetOrdersRpd30082FromDb();
                orders.AddRange(rpd30082);
            }
            catch (Exception e)
            {
                Logger.Log.Debug($"На РПД 30082 возникла ошибка. Ошибка: {e}");
            }

            if (orders.Any())
            {
                serviceResponse = this.ExecuteGoodOperation(HttpStatusCodeEnum.StudentListFindGood, orders);
            }
            else
            {
                serviceResponse = this.ExecuteErrorOperation(HttpStatusCodeEnum.BadListNotFound, "Новых приказов нет");
            }

            return serviceResponse;
        }

        /// <summary>
        /// Данный метод возвращает список всех направлений для студента
        /// </summary>
        /// <param name="actionData"></param>
        /// <returns></returns>
        private ServiceResponse ExecuteGetExtraListForStudent(ActionData actionData)
        {
            var serviceResponse = new ServiceResponse();
            var structList = new List<JsonExtraListForStudent>();

            if (actionData.NrecOneRecord.Length == 0)
            {
                Logger.Log.Info($"От клиента пришел пустой Nrec студента");
                serviceResponse = this.ExecuteErrorOperation(
                    HttpStatusCodeEnum.BadNoNrecListFind,
                    "Не удалось определить Nrec студента");
                return serviceResponse;
            }

            try
            {
                structList = this.dbGalaxyAdapter.GetExtraListForStudentFromDb(actionData.NrecOneRecord);
                if (structList.Any())
                {
                    serviceResponse = this.ExecuteGoodOperation(HttpStatusCodeEnum.StudentListFindGood, structList);
                }
                else
                {
                    serviceResponse = this.ExecuteErrorOperation(HttpStatusCodeEnum.BadListNotFound, "У вас нет направлений");
                }
            }
            catch (Exception e)
            {
                Logger.Log.Debug($"Ошибка при поиске ведомости в базе. Ошибка {e}");
                serviceResponse = this.ExecuteErrorOperation(HttpStatusCodeEnum.BadListNotFound, "У вас нет направлений");
            }

            return serviceResponse;
        }

        /// <summary>
        /// Данный метод возвращает список ППС
        /// </summary>
        /// <param name="actionData"></param>
        /// <returns></returns>
        private ServiceResponse ExecuteGetStuffForList(ActionData actionData)
        {
            var serviceResponse = new ServiceResponse();
            var structList = new List<ListEmployee>();

            try
            {
                if (actionData.NrecOneRecord.Length == 0)
                {
                    Logger.Log.Info($"От клиента пришел пустой Nrec ведомости");
                    serviceResponse = this.ExecuteErrorOperation(
                        HttpStatusCodeEnum.BadNoNrecListFind,
                        "Не удалось определить Nrec ведомости");
                    return serviceResponse;
                }

                if (!_dbAdapterLocalDb.CheckAccessToWorkWithList(actionData.ApiKey, actionData.NrecOneRecord))
                {
                    serviceResponse = this.ExecuteErrorOperation(HttpStatusCodeEnum.Forbidden, "Вам запрещены операции с данным элементом!");
                    return serviceResponse;
                }

                structList = this.dbGalaxyAdapter.GetStuffFromDb(actionData);
                if (structList.Any())
                {
                    serviceResponse = this.ExecuteGoodOperation(HttpStatusCodeEnum.GlobalSuccess, structList);
                }
                else
                {
                    serviceResponse = this.ExecuteErrorOperation(HttpStatusCodeEnum.BadListNotFound, "Не удалось собрать список ППС");
                }
            }
            catch (Exception e)
            {
                Logger.Log.Debug($"Ошибка при поиске оценок в каталоге. Ошибка {e}");
                serviceResponse = this.ExecuteErrorOperation(HttpStatusCodeEnum.BadListNotFound, "Не удалось собрать список ППС");
            }

            return serviceResponse;
        }

        /// <summary>
        /// Данный метод выполняет обновление или вставку списка экзаменаторов, в том числе ответственного
        /// </summary>
        /// <param name="actionData"></param>
        /// <returns></returns>
        private ServiceResponse ExecuteUpdateExaminerByJsonData(ActionData actionData)
        {
            var serviceResponse = new ServiceResponse();
            var structList = new JsonStructList();

            try
            {
                structList = (JsonStructList)this.jsonParser.ParseJson(actionData);
            }
            catch (Exception e)
            {
                Logger.Log.Debug($"Ошибка при парсинге JSON для изъятия информации для обновления данных по экзаменаторам. Ошибка {e}");
                serviceResponse = this.ExecuteErrorOperation(HttpStatusCodeEnum.BadNoNrecListFind, "Не удалось сделать десерилизацию объекта");
                return serviceResponse;
            }

            //Выполняется проверка, что данный клиент может выполнять операции с ведомостями
            if (!_dbAdapterLocalDb.CheckAccessToWorkWithList(actionData.ApiKey, structList.NrecString))
            {
                serviceResponse = this.ExecuteErrorOperation(HttpStatusCodeEnum.Forbidden, "Вам запрещены операции с данным элементом!");
                return serviceResponse;
            }

            if (structList.NrecString.Length == 0)
            {
                Logger.Log.Info($"От клиента пришел пустой Nrec ведомости");
                serviceResponse = this.ExecuteErrorOperation(
                    HttpStatusCodeEnum.BadNoNrecListFind,
                    "Не удалось определить Nrec ведомости");
                return serviceResponse;
            }

            try
            {
                var result = this.dbGalaxyAdapter.UpdateExaminerIntoDb(structList);
                if (!result)
                {
                    serviceResponse = this.ExecuteErrorOperation(HttpStatusCodeEnum.GlobalError, "Не удалось обновить экзаменаторов. Дальнейшее сохранение невозможно");
                }
                else
                {
                    serviceResponse = this.ExecuteGoodOperation(
                        HttpStatusCodeEnum.GlobalSuccess,
                        "Экзаменаторы обновлены");


                }

            }
            catch (Exception e)
            {
                Logger.Log.Debug($"Ошибка сохранении данных в галактику. Ошибка {e}");
                serviceResponse = this.ExecuteErrorOperation(HttpStatusCodeEnum.GlobalError, "Ошибки при сохранении в таблицу экзаменаторов.");
            }

            return serviceResponse;
        }


        /// <summary>
        /// Данный метод возвращает структуру направления
        /// </summary>
        /// <param name="actionData"></param>
        /// <returns></returns>
        private ServiceResponse ExecuteGetExtraListStructureOfListByNrecList(ActionData actionData)
        {
            var serviceResponse = new ServiceResponse();
            var structList = new JsonStructList();

            try
            {
                if (actionData.NrecOneRecord.Length == 0)
                {
                    structList.NrecString = string.Empty;
                }
                else
                {
                    structList.NrecString = actionData.NrecOneRecord.Trim();
                    structList.Nrec = _dataOperation.StringHexToByteArray(structList.NrecString);
                }
            }
            catch (Exception e)
            {
                Logger.Log.Debug($"Ошибка при парсинге JSON для изъятия nrec ведомости, по которой ищются студенты. Ошибка {e}");
                serviceResponse = this.ExecuteErrorOperation(HttpStatusCodeEnum.BadNoNrecListFind, "Не удалось определить Nrec ведомости");
                return serviceResponse;
            }

            //Выполняется проверка, что данный клиент может выполнять операции с ведомостями
            if (!_dbAdapterLocalDb.CheckAccessToWorkWithList(actionData.ApiKey, structList.NrecString))
            {
                serviceResponse = this.ExecuteErrorOperation(HttpStatusCodeEnum.Forbidden, "Вам запрещены операции с данным элементом!");
                return serviceResponse;
            }

            if (structList.NrecString.Length == 0)
            {
                Logger.Log.Info($"От клиента пришел пустой Nrec ведомости");
                serviceResponse = this.ExecuteErrorOperation(
                    HttpStatusCodeEnum.BadNoNrecListFind,
                    "Не удалось определить Nrec ведомости");
                return serviceResponse;
            }

            try
            {
                structList = this.dbGalaxyAdapter.GetListByNrec(structList);
                if (structList.NumDoc != string.Empty)
                {
                    structList.Student = this.dbGalaxyAdapter.GetStudentFromExtraListByNrecListFromDb(structList.Nrec);
                    structList.ListExaminer = this.dbGalaxyAdapter.GetListExaminerFromListByNrecList(structList.Nrec);

                    try
                    {
                        if ((structList.NrecKursList != null && structList.NrecKursList.SequenceEqual(_dataOperation.GetNrecNull) == false) || structList.NrecKursListOther.Any())
                        {
                            structList = this.dbGalaxyAdapter.GetMarkStudentOfKursListRelativeMainList(structList);
                        }
                    }
                    catch (Exception)
                    {
                        //ignore
                    }

                    serviceResponse = this.ExecuteGoodOperation(HttpStatusCodeEnum.StudentListFindGood, structList);
                }
                else
                {
                    serviceResponse = this.ExecuteErrorOperation(HttpStatusCodeEnum.BadListNotFound, "Не удалось найти ведомость");
                }
            }
            catch (Exception e)
            {
                Logger.Log.Debug($"Ошибка при поиске ведомости в базе. Ошибка {e}");
                serviceResponse = this.ExecuteErrorOperation(HttpStatusCodeEnum.BadListNotFound, "Не удалось найти ведомость");
            }

            return serviceResponse;
        }

        /// <summary>
        /// Данный метод пренданзначен для получения списка направлений по FNPP пользователя
        /// </summary>
        /// <param name="actionData"></param>
        /// <returns></returns>
        private ServiceResponse ExecuteExtraListByFnppOperation(ActionData actionData)
        {
            var serviceResponse = new ServiceResponse();

            if (actionData.UserFnpp.Length == 0)
            {
                Logger.Log.Info($"От клиента пришел пустой fnpp");
                serviceResponse = this.ExecuteErrorOperation(HttpStatusCodeEnum.GlobalError, "Не удалось определить fnpp пользователя");
                return serviceResponse;
            }

            var checkExistPerson = this.dbPriemAdapter.CheckExistPersonByFnpp(actionData.UserFnpp);
            if (checkExistPerson is fdata)
            {
                var person = (fdata)checkExistPerson;

                if (!person.keylinks.Any())
                {
                    return this.ExecuteErrorOperation(HttpStatusCodeEnum.GlobalError, "По данному преподавателю нет сопоставлений");
                }

                var teacherLists = this.dbGalaxyAdapter.GetExtraListOfTeacherByGalUnidFromDb(person.keylinks);
                if (teacherLists.Any())
                {
                    var listAllNrec = new List<string>();
                    listAllNrec = teacherLists.Select(r => r.NrecString).ToList();
                    var resultInsertIntoLocalDB = _dbAdapterLocalDb.InsertListNrecIntoAccessTable(actionData.ApiKey, listAllNrec);
                    if (resultInsertIntoLocalDB)
                    {
                        serviceResponse = this.ExecuteGoodOperation(HttpStatusCodeEnum.FnppGoodFindTeacherList, teacherLists);
                    }
                    else
                    {
                        serviceResponse = this.ExecuteErrorOperation(HttpStatusCodeEnum.GlobalError, "Ошибка локальной базы");
                    }

                    // Logger.Log.Debug(serviceResponse.StringResponse);
                }
                else
                {
                    return this.ExecuteErrorOperation(HttpStatusCodeEnum.GlobalError, "Для данного преподавателя нет ведомостей");
                }
            }
            else
            {
                Logger.Log.Info($"По данному fnpp не удалось найти пользователя в базе");
                serviceResponse = this.createResponse.GenerateErrorResponse(HttpStatusCodeEnum.GlobalError, "Пользователя с таким fnpp не существует");
                return serviceResponse;
            }


            return serviceResponse;
        }

        /// <summary>
        /// Данный метод производит встаку или удаление тем курсовых работ
        /// </summary>
        /// <param name="actionData"></param>
        /// <returns></returns>
        private ServiceResponse ExecuteModifeKursTheme(ActionData actionData)
        {
            var serviceResponse = new ServiceResponse();

            var structJsonKursTheme = new JsonKursTheme();

            try
            {
                structJsonKursTheme = (JsonKursTheme)this.jsonParser.ParseJson(actionData);
            }
            catch (Exception e)
            {
                Logger.Log.Debug($"Ошибка при парсинге JSON для изъятия информации для обновления курсовых работ. Ошибка {e}");
                serviceResponse = this.ExecuteErrorOperation(HttpStatusCodeEnum.BadNoNrecListFind, "Не удалось сделать десерилизацию объекта");
                return serviceResponse;
            }

            //Выполняется проверка, что данный клиент может выполнять операции с ведомостями
            if (!_dbAdapterLocalDb.CheckAccessToWorkWithList(actionData.ApiKey, structJsonKursTheme.NrecString))
            {
                serviceResponse = this.ExecuteErrorOperation(HttpStatusCodeEnum.Forbidden, "Вам запрещены операции с данным элементом!");
                return serviceResponse;
            }


            if (structJsonKursTheme.NrecString.Length == 0)
            {
                Logger.Log.Info($"От клиента пришел пустой Nrec ведомости");
                serviceResponse = this.ExecuteErrorOperation(
                    HttpStatusCodeEnum.BadNoNrecListFind,
                    "Не удалось определить Nrec ведомости");
                return serviceResponse;
            }

            try
            {
                var resultkursThemeModife = this.dbGalaxyAdapter.ModifeKursThemeByMarkNrecIntoDb(structJsonKursTheme);
                if (!resultkursThemeModife)
                {
                    serviceResponse = this.ExecuteErrorOperation(HttpStatusCodeEnum.GlobalError, "Не удалось обновить или вставить темы курсовых работ. Дальнейшее сохранение невозможно");
                }
                else
                {
                    serviceResponse = this.ExecuteGoodOperation(
                        HttpStatusCodeEnum.GlobalSuccess,
                        "Темы курсовых работ успешно обновлены");

                }

            }
            catch (Exception e)
            {
                Logger.Log.Debug($"Ошибка сохранении данных в галактику. Ошибка {e}");
                serviceResponse = this.ExecuteErrorOperation(HttpStatusCodeEnum.GlobalError, "Ошибки при сохранении тем курсовых работ.");
            }

            return serviceResponse;
        }

        /// <summary>
        /// Данный метод получает темы курсовых работ
        /// </summary>
        /// <param name="actionData"></param>
        /// <returns></returns>
        private ServiceResponse ExecuteGetKursThemes(ActionData actionData)
        {
            var serviceResponse = new ServiceResponse();
            var structListKursTheme = new JsonKursTheme();

            try
            {
                structListKursTheme.NrecString = actionData.NrecOneRecord;
                structListKursTheme.Nrec = _dataOperation.StringHexToByteArray(structListKursTheme.NrecString);
            }
            catch (Exception e)
            {
                Logger.Log.Debug($"При запросе на выбор тем курсовых работ пришел плохой nrec ведомости. Ошибка {e}");
                serviceResponse = this.ExecuteErrorOperation(HttpStatusCodeEnum.BadNoNrecListFind, "Не удалось определить Nrec ведомости");
                return serviceResponse;
            }

            //Выполняется проверка, что данный клиент может выполнять операции с ведомостями
            if (!_dbAdapterLocalDb.CheckAccessToWorkWithList(actionData.ApiKey, structListKursTheme.NrecString))
            {
                serviceResponse = this.ExecuteErrorOperation(HttpStatusCodeEnum.Forbidden, "Вам запрещены операции с данным элементом!");
                return serviceResponse;
            }

            if (structListKursTheme.Nrec.SequenceEqual(_dataOperation.GetNrecNull))
            {
                Logger.Log.Info($"От клиента пришел пустой Nrec ведомости");
                serviceResponse = this.ExecuteErrorOperation(
                    HttpStatusCodeEnum.BadNoNrecListFind,
                    "Не удалось определить Nrec ведомости");
                return serviceResponse;
            }

            try
            {
                structListKursTheme = this.dbGalaxyAdapter.GetListByNrecForKursTheme(structListKursTheme);
                if (structListKursTheme.NumDoc.Length != 0)
                {
                    structListKursTheme.Student = this.dbGalaxyAdapter.GetStudentsKursThemeByNrecList(structListKursTheme.Nrec);
                    structListKursTheme.ListExaminer = this.dbGalaxyAdapter.GetListExaminerFromListByNrecList(structListKursTheme.Nrec);

                    serviceResponse = this.ExecuteGoodOperation(HttpStatusCodeEnum.StudentListFindGood, structListKursTheme);
                }
                else
                {
                    serviceResponse = this.ExecuteErrorOperation(HttpStatusCodeEnum.BadListNotFound, "Не удалось найти ведомость для курсовой работы");
                }
            }
            catch (Exception e)
            {
                Logger.Log.Debug($"Ошибка при поиске ведомости для курсовой работы в базе. Ошибка {e}");
                serviceResponse = this.ExecuteErrorOperation(HttpStatusCodeEnum.BadListNotFound, "Не удалось найти ведомость для курсовой работы");
            }

            return serviceResponse;

        }

        /// <summary>
        /// Данный метод удаляет запись из таблицы
        /// </summary>
        /// <param name="actionData"></param>
        /// <returns></returns>
        private ServiceResponse ExecuteDeleteRecordFromTable(ActionData actionData)
        {
            var serviceResponse = new ServiceResponse();
            var result = false;
            try
            {
                result = this.dbGalaxyAdapter.DeleteRecordFromTable(actionData.RecordForDelete);
                if (result)
                {
                    serviceResponse = this.ExecuteGoodOperation(HttpStatusCodeEnum.GlobalSuccess, "Запись успешно удалена");
                }
                else
                {
                    serviceResponse = this.ExecuteErrorOperation(HttpStatusCodeEnum.GlobalError, "Не удалось удалить запись");
                }
            }
            catch (Exception e)
            {
                Logger.Log.Debug($"Не удалось удалить запись. Ошибка {e}");
                serviceResponse = this.ExecuteErrorOperation(HttpStatusCodeEnum.GlobalError, "Не удалось удалить запись");
            }

            return serviceResponse;
        }

        /// <summary>
        /// Данный метод возвращает актуальный список студентов
        /// </summary>
        /// <param name="actionData"></param>
        /// <returns></returns>
        private ServiceResponse ExecuteGetAllStudentsByJsonData(ActionData actionData)
        {
            var serviceResponse = new ServiceResponse();
            var structList = new List<ListStudent>();

            if (GlobalSettings.Instance.AuthDataGalLogin == "OMGTU910#mobilephone" && string.IsNullOrEmpty(actionData.QueryParamNum1))
            {
                Logger.Log.Debug($"Мобильному приложению запрещено получать весь список студентов");
                serviceResponse = this.ExecuteErrorOperation(HttpStatusCodeEnum.GlobalError, "Мобильному приложению запрещено получать весь список студентов");
                return serviceResponse;
            }
            if (!string.IsNullOrEmpty(actionData.QueryParamNum1) && actionData.QueryParamNum1.Equals("0"))
            {
                Logger.Log.Debug($"fnpp не может быть равен нулю (0)");
                serviceResponse = this.ExecuteErrorOperation(HttpStatusCodeEnum.GlobalError, "fnpp не может быть равен нулю (0)");
                return serviceResponse;
            }
            try
            {

                try
                {
                    var jsonMemory = new JsonStudents()
                    {
                        Hash = string.Empty,
                        Students = structList
                    };

                    if (_cache.TryGetValue(actionData.RequestBodyMD5, out jsonMemory))
                    {
                        Logger.Log.Info($"Результат по запросу карточек студентов отправлен из кэша.");
                        serviceResponse = this.ExecuteGoodOperation(HttpStatusCodeEnum.GlobalSuccess, jsonMemory);
                        return serviceResponse;
                    }
                }
                catch (Exception e)
                {
                    Logger.Log.Info($"Ошибка обращения к кэшу памяти. Ошибка {e}");
                }



                var studentNrec = new List<byte[]>();
                if (!string.IsNullOrEmpty(actionData.QueryParamNum1))
                {
                    studentNrec = this.dbPriemAdapter.GetAllStudentNrecByFnpp(actionData.QueryParamNum1);
                }


                structList = this.dbGalaxyAdapter.GetAllStudents(studentNrec);
                if (structList.Any())
                {
                    var hash = _dataOperation.CreateMD5(structList.ToString());
                    var jsonReturn = new JsonStudents()
                    {
                        Hash = hash,
                        Students = structList
                    };

                    _cache.Set(actionData.RequestBodyMD5, jsonReturn,
                        new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(30)));
                    Logger.Log.Info("Результат выборки по карточкам студентов внесен в кэш");

                    serviceResponse = this.ExecuteGoodOperation(HttpStatusCodeEnum.GlobalSuccess, jsonReturn);
                }
                else
                {
                    serviceResponse = this.ExecuteErrorOperation(HttpStatusCodeEnum.BadListNotFound, "Не удалось собрать список студентов");
                }
            }
            catch (Exception e)
            {
                Logger.Log.Debug($"Ошибка при сборе списка студентов. Ошибка {e}");
                serviceResponse = this.ExecuteErrorOperation(HttpStatusCodeEnum.BadListNotFound, "Не удалось собрать список оценок");
            }

            return serviceResponse;
        }

        /// <summary>
        /// Данный метод возвращает перчень наименование оценко из каталога (отлично, хорошо и т.д.)
        /// </summary>
        /// <param name="actionData"></param>
        /// <returns></returns>
        private ServiceResponse ExecuteGetCatalogsMarks(ActionData actionData)
        {
            var serviceResponse = new ServiceResponse();
            var structList = new List<JsonCatalogMarks>();

            try
            {

                try
                {
                    if (_cache.TryGetValue("catalogMarks", out structList))
                    {
                        Logger.Log.Info($"Результат по запросу каталога оценок отправлен из кэша.");
                        serviceResponse = this.ExecuteGoodOperation(HttpStatusCodeEnum.GlobalSuccess, structList);
                        return serviceResponse;
                    }
                }
                catch (Exception e)
                {
                    Logger.Log.Info($"Ошибка обращения к кэшу памяти. Ошибка {e}");
                }




                structList = this.dbGalaxyAdapter.GetGatalogMarksFromDb();
                if (structList.Any())
                {
                    _cache.Set("catalogMarks", structList,
                        new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromDays(1)));
                    Logger.Log.Info("Результат выборки по каталогу оценок внесен в кэш");
                    serviceResponse = this.ExecuteGoodOperation(HttpStatusCodeEnum.GlobalSuccess, structList);
                }
                else
                {
                    serviceResponse = this.ExecuteErrorOperation(HttpStatusCodeEnum.BadListNotFound, "Не удалось собрать каталог оценок");
                }
            }
            catch (Exception e)
            {
                Logger.Log.Debug($"Ошибка при поиске оценок в каталоге. Ошибка {e}");
                serviceResponse = this.ExecuteErrorOperation(HttpStatusCodeEnum.BadListNotFound, "Не удалось собрать каталог оценок");
            }

            return serviceResponse;
        }

        /// <summary>
        /// Данный метод выполнения обновления рейтинга и оценок пользователя
        /// </summary>
        /// <param name="actionData">
        /// The action data.
        /// </param>
        /// <returns>
        /// The <see cref="ServiceResponse"/>.
        /// </returns>
        private ServiceResponse ExecuteUpdateMarkAndRatinByJsonData(ActionData actionData)
        {
            var serviceResponse = new ServiceResponse();
            var structList = new JsonStructList();

            try
            {
                structList = (JsonStructList)this.jsonParser.ParseJson(actionData);
            }
            catch (Exception e)
            {
                Logger.Log.Debug($"Ошибка при парсинге JSON для изъятия информации для обновления данных по студентам. Ошибка {e}");
                serviceResponse = this.ExecuteErrorOperation(HttpStatusCodeEnum.BadNoNrecListFind, "Не удалось сделать десерилизацию объекта");
                return serviceResponse;
            }

            //Выполняется проверка, что данный клиент может выполнять операции с ведомостями
            if (!_dbAdapterLocalDb.CheckAccessToWorkWithList(actionData.ApiKey, structList.NrecString))
            {
                serviceResponse = this.ExecuteErrorOperation(HttpStatusCodeEnum.Forbidden, "Вам запрещены операции с данным элементом!");
                return serviceResponse;
            }

            if (structList.NrecString.Length == 0)
            {
                Logger.Log.Info($"От клиента пришел пустой Nrec ведомости");
                serviceResponse = this.ExecuteErrorOperation(
                    HttpStatusCodeEnum.BadNoNrecListFind,
                    "Не удалось определить Nrec ведомости");
                return serviceResponse;
            }

            try
            {
                var resultMarkAndRatingUpdate = this.dbGalaxyAdapter.UpdateMarkAndRaingByNrecTableMark(structList);
                if (!resultMarkAndRatingUpdate)
                {
                    serviceResponse = this.ExecuteErrorOperation(HttpStatusCodeEnum.GlobalError, "Не удалось обновить оценки и рейтинг. Дальнейшее сохранение невозможно");
                }
                else
                {
                    if (structList.DateList != 0 || structList.DopStatusList != null)
                    {
                        var resultUpdateDateAndDopStatus = this.dbGalaxyAdapter.UpdateDateAndDopStatus(structList);
                        serviceResponse = this.ExecuteGoodOperation(HttpStatusCodeEnum.GlobalSuccess, resultUpdateDateAndDopStatus ? "Оценки, рейтинги и информации по ведомости успешно обновлены" : "Оценки обновлены, однако при обновлении статуса ведомости и даты произошли ошибки");
                    }
                    else
                    {
                        serviceResponse = this.ExecuteGoodOperation(
                            HttpStatusCodeEnum.GlobalSuccess,
                            "Оценки и рейтинги успешно обновлены");
                    }

                }

            }
            catch (Exception e)
            {
                Logger.Log.Debug($"Ошибка сохранении данных в галактику. Ошибка {e}");
                serviceResponse = this.ExecuteErrorOperation(HttpStatusCodeEnum.GlobalError, "Ошибки при сохранении в таблицу оценок.");
            }

            return serviceResponse;
        }

        /// <summary>
        /// Данный метод выполняет обновление рейтинга и часов
        /// </summary>
        /// <param name="actionData">
        /// The action data.
        /// </param>
        /// <returns>
        /// The <see cref="ServiceResponse"/>.
        /// </returns>
        private ServiceResponse ExecuteUpdateFieldRatingHoursByJsonData(ActionData actionData)
        {
            var serviceResponse = new ServiceResponse();
            var structList = new JsonStructList();

            try
            {
                structList = (JsonStructList)this.jsonParser.ParseJson(actionData);
            }
            catch (Exception e)
            {
                Logger.Log.Debug($"Ошибка при парсинге JSON для изъятия информации для обновления данных по студентам. Ошибка {e}");
                serviceResponse = this.ExecuteErrorOperation(HttpStatusCodeEnum.BadNoNrecListFind, "Не удалось сделать десерилизацию объекта");
                return serviceResponse;
            }

            //Выполняется проверка, что данный клиент может выполнять операции с ведомостями
            if (!_dbAdapterLocalDb.CheckAccessToWorkWithList(actionData.ApiKey, structList.NrecString))
            {
                serviceResponse = this.ExecuteErrorOperation(HttpStatusCodeEnum.Forbidden, "Вам запрещены операции с данным элементом!");
                return serviceResponse;
            }

            if (structList.NrecString.Length == 0)
            {
                Logger.Log.Info($"От клиента пришел пустой Nrec ведомости");
                serviceResponse = this.ExecuteErrorOperation(
                    HttpStatusCodeEnum.BadNoNrecListFind,
                    "Не удалось определить Nrec ведомости");
                return serviceResponse;
            }

            try
            {
                var resultListHoursUpdate = this.dbGalaxyAdapter.UpdateListHoursByNrec(structList);
                if (!resultListHoursUpdate)
                {
                    serviceResponse = this.ExecuteErrorOperation(HttpStatusCodeEnum.GlobalError, "Не удалось обновить часы по ведомости. Дальнейшее сохранение невозможно");
                }

                if (structList.Student.Any())
                {
                    var resultStudListHoursRatingUpdate = this.dbGalaxyAdapter.UpdateStudListHoursRating(structList);
                    if (resultStudListHoursRatingUpdate)
                    {
                        serviceResponse = this.ExecuteGoodOperation(
                            HttpStatusCodeEnum.GlobalSuccess,
                            "Данные успешно обновлены");
                    }
                    else
                    {
                        serviceResponse = this.ExecuteErrorOperation(HttpStatusCodeEnum.GlobalError,
                            "Не удалось обновить часы по студентам.");
                    }
                }
                else
                {
                    serviceResponse = this.ExecuteGoodOperation(HttpStatusCodeEnum.GlobalSuccess,
                        "Часы ведомости успешно обновлены");
                }

            }
            catch (Exception e)
            {
                Logger.Log.Debug($"Ошибка сохранении данных в галактику. Ошибка {e}");
                serviceResponse = this.ExecuteErrorOperation(HttpStatusCodeEnum.GlobalError, "Ошибки при сохранении ведомости.");
            }

            return serviceResponse;
        }

        /// <summary>
        /// Данный метод возвращает структуру ведомости по ее Nrec
        /// </summary>
        /// <param name="actionData">
        /// The action data.
        /// </param>
        /// <returns>
        /// The <see cref="ServiceResponse"/>.
        /// </returns>
        private ServiceResponse ExecuteGetStructureOfListByNrecList(ActionData actionData)
        {
            var serviceResponse = new ServiceResponse();
            var structList = new JsonStructList();

            try
            {
                structList = (JsonStructList)this.jsonParser.ParseJson(actionData);
            }
            catch (Exception e)
            {
                Logger.Log.Error($"Ошибка при парсинге JSON для изъятия nrec ведомости, по которой ищются студенты. Ошибка {e}");
                serviceResponse = this.ExecuteErrorOperation(HttpStatusCodeEnum.BadNoNrecListFind, "Не удалось определить Nrec ведомости");
                return serviceResponse;
            }

            //Выполняется проверка, что данный клиент может выполнять операции с ведомостями
            if (!_dbAdapterLocalDb.CheckAccessToWorkWithList(actionData.ApiKey, structList.NrecString))
            {
                serviceResponse = this.ExecuteErrorOperation(HttpStatusCodeEnum.Forbidden, "Вам запрещены операции с данным элементом!");
                return serviceResponse;
            }

            if (structList.NrecString.Length == 0)
            {
                Logger.Log.Info($"От клиента пришел пустой Nrec ведомости");
                serviceResponse = this.ExecuteErrorOperation(
                    HttpStatusCodeEnum.BadNoNrecListFind,
                    "Не удалось определить Nrec ведомости");
                return serviceResponse;
            }

            try
            {
                structList = this.dbGalaxyAdapter.GetListByNrec(structList);
                if (structList.NumDoc != string.Empty)
                {
                    try
                    {
                        structList.Student = this.dbGalaxyAdapter.GetStudentFromListByNrecList(structList.Nrec);
                    }
                    catch (Exception e)
                    {
                        Logger.Log.Error($"При формировании списка студентов для ведомости произошла ошибка. Ошибка {e}");
                    }

                    structList.ListExaminer = this.dbGalaxyAdapter.GetListExaminerFromListByNrecList(structList.Nrec);

                    try
                    {
                        if ((structList.NrecKursList != null && structList.NrecKursList.SequenceEqual(_dataOperation.GetNrecNull) == false) || structList.NrecKursListOther.Any())
                        {
                            structList = this.dbGalaxyAdapter.GetMarkStudentOfKursListRelativeMainList(structList);
                        }
                    }
                    catch (Exception)
                    {
                        //ignore
                    }

                    serviceResponse = this.ExecuteGoodOperation(HttpStatusCodeEnum.StudentListFindGood, structList);
                }
                else
                {
                    serviceResponse = this.ExecuteErrorOperation(HttpStatusCodeEnum.BadListNotFound, "Не удалось найти ведомость");
                }
            }
            catch (Exception e)
            {
                Logger.Log.Error($"Ошибка при поиске ведомости в базе. Ошибка {e}");
                serviceResponse = this.ExecuteErrorOperation(HttpStatusCodeEnum.BadListNotFound, "Не удалось найти ведомость");
            }

            return serviceResponse;
        }

        /// <summary>
        /// Данный метод получает все ведомости преподавателя по его fnpp 
        /// </summary>
        /// <param name="actionData"></param>
        /// <returns></returns>
        private ServiceResponse ExecuteAllListByFnppOperation(ActionData actionData)
        {
            var serviceResponse = new ServiceResponse();
            var teacher = new JsonFnpp();
            try
            {
                teacher = (JsonFnpp)this.jsonParser.ParseJson(actionData);
            }
            catch (Exception e)
            {
                Logger.Log.Error($"Ошибка при парсинге JSON для изъятия fnpp клиента. Ошибка {e}");
                serviceResponse = this.ExecuteErrorOperation(HttpStatusCodeEnum.GlobalError, "Не удалось определить fnpp пользователя");
                return serviceResponse;
            }

            if (teacher.Fnpp == string.Empty)
            {
                Logger.Log.Info($"От клиента пришел пустой fnpp");
                serviceResponse = this.ExecuteErrorOperation(HttpStatusCodeEnum.GlobalError, "Не удалось определить fnpp пользователя");
                return serviceResponse;
            }

            var checkExistPerson = this.dbPriemAdapter.CheckExistPersonByFnpp(teacher.Fnpp);
            if (checkExistPerson is fdata)
            {
                var person = (fdata)checkExistPerson;

                if (!person.keylinks.Any())
                {
                    return this.ExecuteErrorOperation(HttpStatusCodeEnum.GlobalError, "По данному преподавателю нет сопоставлений");
                }

                var isChief = dbPriemAdapter.CheckChiefPersonByGalUnid(person.keylinks);

                var teacherLists = this.dbGalaxyAdapter.GetListOfTeacherByGalUnid(person.keylinks, isChief);
                if (teacherLists.Any())
                {
                    var listAllNrec = new List<string>();
                    listAllNrec = teacherLists.Select(r => r.NrecString).ToList();
                    var resultInsertIntoLocalDB = _dbAdapterLocalDb.InsertListNrecIntoAccessTable(actionData.ApiKey, listAllNrec);
                    if (resultInsertIntoLocalDB)
                    {
                        serviceResponse = this.ExecuteGoodOperation(HttpStatusCodeEnum.FnppGoodFindTeacherList, teacherLists);
                    }
                    else
                    {
                        serviceResponse = this.ExecuteErrorOperation(HttpStatusCodeEnum.GlobalError, "Ошибка локальной базы");
                    }

                    // Logger.Log.Debug(serviceResponse.StringResponse);
                }
                else
                {
                    return this.ExecuteErrorOperation(HttpStatusCodeEnum.GlobalError, "Для данного преподавателя нет ведомостей");
                }
            }
            else
            {
                Logger.Log.Info($"По данному fnpp не удалось найти пользователя в базе");
                serviceResponse = this.createResponse.GenerateErrorResponse(HttpStatusCodeEnum.GlobalError, "Пользователя с таким fnpp не существует");
                return serviceResponse;
            }


            return serviceResponse;
        }

        private ServiceResponse ExecuteErrorOperation(HttpStatusCodeEnum code, string error)
        {
            var response = this.createResponse.GenerateErrorResponse(code, error);
            return response;
        }

        private ServiceResponse ExecuteGoodOperation(HttpStatusCodeEnum code, object result)
        {
            var response = this.createResponse.GenerateGoodResponse(code, result);
            return response;
        }

        /// <summary>
        /// Обновление информации по практике в ведомости
        /// </summary>
        /// <param name="actionData">
        /// The action data.
        /// </param>
        /// <returns>
        /// The <see cref="ServiceResponse"/>.
        /// </returns>
        private ServiceResponse ExecuteUpdatePracticeListByJsonData(ActionData actionData)
        {
            var serviceResponse = new ServiceResponse();
            var structList = new JsonPracticeList();

            try
            {
                structList = (JsonPracticeList)this.jsonParser.ParseJson(actionData);
            }
            catch (Exception e)
            {
                Logger.Log.Debug($"Ошибка при парсинге JSON для изъятия информации для обновления данных по практике. Ошибка {e}");
                serviceResponse = this.ExecuteErrorOperation(HttpStatusCodeEnum.BadNoNrecListFind, "Не удалось сделать десерилизацию объекта");
                return serviceResponse;
            }

            if (structList.ListNrecString.Length == 0 || (structList.ListNrecString.Equals("0x8000000000000000")))
            {
                Logger.Log.Info($"От клиента пришел пустой Nrec ведомости по практике");
                serviceResponse = this.ExecuteErrorOperation(
                    HttpStatusCodeEnum.BadNoNrecListFind,
                    "Не удалось определить Nrec ведомости по практике");
                return serviceResponse;
            }
            try
            {
                var resultPracticeUpdate = this.dbGalaxyAdapter.UpdatePracticeList(structList);
                if (!resultPracticeUpdate)
                {
                    serviceResponse = this.ExecuteErrorOperation(HttpStatusCodeEnum.GlobalError, "Не удалось обновить информацию о прохождении практики. Дальнейшее сохранение невозможно");
                }
                else
                {
                    serviceResponse = this.ExecuteGoodOperation(HttpStatusCodeEnum.GlobalSuccess, "Информация по практике успешно обновлена");
                }
            }
            catch (Exception e)
            {
                Logger.Log.Debug($"Ошибка при сохранении данных в галактику. Ошибка {e}");
                serviceResponse = this.ExecuteErrorOperation(HttpStatusCodeEnum.GlobalError, "Ошибки при сохранении в таблицу информации по практике.");
            }
            return serviceResponse;
        }

        /// <summary>
        /// Получение информации по местам практики
        /// </summary>
        private ServiceResponse ExecuteGetEnterpriseList()
        {
            var serviceResponse = new ServiceResponse();
            var structList = new List<JsonEnterprises>();

            try
            {
                structList = this.dbGalaxyAdapter.GetEnterpriseList();
                serviceResponse = this.ExecuteGoodOperation(HttpStatusCodeEnum.GlobalSuccess, structList);
            }
            catch (Exception e)
            {
                Logger.Log.Debug($"Ошибка при получении информации о предприятиях. Ошибка {e}");
                serviceResponse = this.ExecuteGoodOperation(HttpStatusCodeEnum.GlobalError, "Ошибка при получении информации о предприятиях");
            }
            return serviceResponse;
        }

        /// <summary>
        /// Обновление информации/добавление места практики
        /// </summary>
        private ServiceResponse ExecuteUpdateEnterprise(ActionData actionData)
        {
            var serviceResponse = new ServiceResponse();
            var structList = new JsonEnterprises();

            try
            {
                structList = (JsonEnterprises)this.jsonParser.ParseJson(actionData);
            }
            catch (Exception e)
            {
                Logger.Log.Debug($"Ошибка при парсинге JSON для изъятия информации для обновления предприятия. Ошибка {e}");
                serviceResponse = this.ExecuteErrorOperation(HttpStatusCodeEnum.BadNoNrecListFind, "Не удалось сделать десерилизацию объекта");
                return serviceResponse;
            }

            if (structList.NrecString.Length == 0)
            {
                Logger.Log.Info($"От клиента пришел пустой Nrec предприятия");
                serviceResponse = this.ExecuteErrorOperation(
                    HttpStatusCodeEnum.BadNoNrecListFind,
                    "Не удалось определить Nrec предприятия");
                return serviceResponse;
            }
            try
            {
                var resultPracticeUpdate = this.dbGalaxyAdapter.UpdateEnterprise(structList);
                if (!resultPracticeUpdate)
                {
                    serviceResponse = this.ExecuteErrorOperation(HttpStatusCodeEnum.GlobalError, "Не удалось обновить информацию о предприятии. Дальнейшее сохранение невозможно");
                }
                else
                {
                    serviceResponse = this.ExecuteGoodOperation(HttpStatusCodeEnum.GlobalSuccess, "Информация успешно обновлена");
                }
            }
            catch (Exception e)
            {
                Logger.Log.Debug($"Ошибка при сохранении данных в галактику. Ошибка {e}");
                serviceResponse = this.ExecuteErrorOperation(HttpStatusCodeEnum.GlobalError, "Ошибки при сохранении в таблицу информации о предприятиях.");
            }
            return serviceResponse;
        }

        /// <summary>
        /// Получение информации о преподавателе
        /// </summary>
        private ServiceResponse ExecuteGetLectureInfo(ActionData actionData)
        {
            var serviceResponse = new ServiceResponse();
            List<JsonLecture> rec = new List<JsonLecture>();
            try
            {
                rec = this.dbPriemAdapter.GetLectureInfo(actionData.QueryParamNum1);
                serviceResponse = this.ExecuteGoodOperation(HttpStatusCodeEnum.GlobalSuccess, rec);
            }
            catch (Exception e)
            {
                Logger.Log.Debug($"Ошибка при получении информации о преподавателе. Ошибка {e}");
                serviceResponse = this.ExecuteGoodOperation(HttpStatusCodeEnum.GlobalError, "Ошибка при получении информации о преподавателе");
            }
            return serviceResponse;
        }
    }
}
