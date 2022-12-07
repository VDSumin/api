using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.Sockets;
using log4net;
using SFAA.Entities;
using SFAA.DataOperation;
using SFAA.DBAdapter;

namespace SFAA.ClientServerOperation
{
    public class Client
    {
        public Client(Func<ActionData, ServiceResponse> actionMethod, TcpClient Client)
        {
            var netStream = Client.GetStream();
            netStream.ReadTimeout = 100;

            byte[] result;

            Logger.Log.Info($"Можем ли читать данные ? {netStream.CanRead}");
            Logger.Log.Info($"Данные доступны ? {netStream.DataAvailable}");
            try
            {
                if (netStream.CanRead) /// && netStream.DataAvailable)
                {
                    Logger.Log.Info($"Начинаем чтение данных");
                    using (var stream = new MemoryStream())
                    {


                        byte[] buffer = new byte[Client.ReceiveBufferSize]; // read in chunks of 2KB
                        int bytesRead;
                        try
                        {
                            do
                            {
                                bytesRead = netStream.Read(buffer, 0, buffer.Length);
                                stream.Write(buffer, 0, bytesRead);
                            }
                            while (netStream.DataAvailable);
                        }
                        catch (Exception)
                        {
                            Logger.Log.Error("Ошибка по таймауту");

                        }

                        result = stream.ToArray();


                    }

                    //Returns the data received from the host to the console.
                    var returndata = Encoding.Default.GetString(result);

                    Logger.Log.Info($"Полученная строка от клиента - {returndata}");

                    var resultParse = new ActionData()
                    {
                        OperationType = OperationTypeEnum.Error
                    };

                    DBAdapterLocalDB dblocal;
                    try
                    {
                        var parseRequest = new ParseRequest(returndata);
                        var apikey = parseRequest.CheckApiKey(returndata);

                        dblocal = new DBAdapterLocalDB();
                        var check = dblocal.GetGalAuthDataByApiKey(apikey);
                        if (check != null && check.Count > 0)
                        {
                            GlobalSettings.Instance.CurrentApiUser.ApiKey = apikey;
                            log4net.ThreadContext.Properties["apikey"] = GlobalSettings.Instance.CurrentApiUser.ApiKey;

                            if (GlobalSettings.Instance.CurrentApiUser.GalStatus == 0)
                            {
                                resultParse = new ActionData()
                                {
                                    RequestString = returndata,
                                    OperationType = OperationTypeEnum.AccessApiKey,
                                };
                            }
                            else
                            {
                                parseRequest.SetApiKey(apikey);
                                dblocal.InsertInfoConnectnioIntoLogConnection(0);
                                foreach (var pair in check)
                                {
                                    GlobalSettings.Instance.AuthDataGalLogin = pair.Key;
                                    GlobalSettings.Instance.AuthDataGalPass = pair.Value;
                                }

                                resultParse = parseRequest.GenerateActionData();
                            }
                        }
                        else
                        {
                            resultParse = new ActionData()
                            {
                                RequestString = returndata,
                                OperationType = OperationTypeEnum.ErrorApiKey,
                            };
                        }

                    }
                    catch (Exception exception)
                    {
                        Logger.Log.Error($"Неудалось выполнить парсинг запроса от клиента. Ошибка = {exception}");
                    }

                    var response = actionMethod.Invoke(resultParse);

                    // Необходимые заголовки: ответ сервера, тип и длина содержимого. После двух пустых строк - само содержимое
                    //string Str = "HTTP/1.1 200 OK\nContent-type: text/html\nContent-Length:" + Html.Length.ToString() + "\n\n" + Html;
                    try
                    {
                        netStream.Write(response.BytesResponse, 0, response.BytesResponse.Length);
                    }
                    catch (Exception e)
                    {
                        netStream.Write(new byte[0], 0, 0);
                    }
                    dblocal = new DBAdapterLocalDB();
                    dblocal.InsertInfoConnectnioIntoLogConnection(1);
                    Logger.Log.Info("--------------------------------------------------------------------");
                    Logger.Log.Info("--------------------------------------------------------------------\n");
                    Logger.Log.Info("Соединение с клиентом закрыто");
                    // Закроем соединение
                    Client.Close();
                }
                else
                {
                    Client.Close();
                }

                if (Client.Connected)
                {
                    Client.Close();
                }
            }
            catch (Exception e)
            {
                Logger.Log.Info("--------------------------------------------------------------------");
                Logger.Log.Info("--------------------------------------------------------------------\n");
                Logger.Log.Info("Соединение с клиентом закрыто");
                Logger.Log.Error($"Соединение с клиентом закрыто из-за ошибки. Ошибка {e}");
                // Закроем соединение
                Client.Close();
            }
            







        }
    }
}
