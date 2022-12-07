using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
 using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using SFAA.Entities;

namespace SFAA.ClientServerOperation
{

    public class Listener
    {
        TcpListener tcpListener;

        private Func<ActionData, ServiceResponse> actionMethod;

        public Listener(Func<ActionData, ServiceResponse> actionMethod)
        {
            this.actionMethod = actionMethod;
            this.tcpListener = new TcpListener(IPAddress.Parse(GlobalSettings.Instance.ServerIp), GlobalSettings.Instance.ListenPort);
            this.tcpListener.Start();
            try
            {
                this.WaitClient();
            }
            catch (Exception e)
            {
                Logger.Log.Error($"Во время запуска слушателя проиозшла ошибка. Ошибка {e}");
                this.tcpListener.Start();
            }
            

        }

        public void WaitClient()
        {
            while (true)
            {
                Logger.Log.Info($"Принимаем клиента");
                // Принимаем нового клиента
                TcpClient Client = this.tcpListener.AcceptTcpClient();
                Logger.Log.Info($"Текущий клиент - {((IPEndPoint)Client.Client.RemoteEndPoint).Address.ToString()}");

#if DEBUG 
                // Создаем поток
                Thread Thread = new Thread(new ParameterizedThreadStart(ClientThread));
                // И запускаем этот поток, передавая ему принятого клиента
                Thread.Start(Client);
                Logger.Log.Info($"Текущий поток создан для клиента - {((IPEndPoint)Client.Client.RemoteEndPoint).Address.ToString()}");
#else
                //if (((IPEndPoint)Client.Client.RemoteEndPoint).Address.ToString() == "195.69.204.45")
                //{
                    // Создаем поток
                    Thread Thread = new Thread(new ParameterizedThreadStart(ClientThread));
                    // И запускаем этот поток, передавая ему принятого клиента
                    Thread.Start(Client);
                    Logger.Log.Info($"Текущий поток создан для клиента - {((IPEndPoint)Client.Client.RemoteEndPoint).Address.ToString()}");
                //}
                //else
                //{
                //    Logger.Log.Info($"Недопустимый адрес клиента!!!");
                //    Client.Close();
                //    Logger.Log.Info("Соединение с клиентом закрыто");
                //}
#endif




            }
        }


        private void ClientThread(Object StateInfo)
        {
            new Client(actionMethod, (TcpClient)StateInfo);
        }

        ~Listener()
        {
            if (this.tcpListener != null)
            {
                this.tcpListener.Stop();
            }
        }
    }
}
