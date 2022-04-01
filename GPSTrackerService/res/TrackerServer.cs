using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using GPSTrackerService.res;
using NLog;

namespace GPSTrackerService
{
    public class TrackerServer
    {
        private Socket _serverSocket;
        private int _port;
        private readonly int _threadLife;
        private static char[] spliter = { '\n', '\r' };
        private int _maxQueue;

        private Dictionary<string, int> IdsSpeed0 = new Dictionary<string, int>();
        private int MaxCountSpeed0 = 3;
        private object _lockIds = new object();

        public Queue<GPSMessage> LastData { get; set; }
        private object _queueLock = new object();

        public TrackerServer(int port, int threadLifeSec, int maxQueue)
        {
            _port = port;
            _threadLife = threadLifeSec * 1000;
            _maxQueue = maxQueue;
            LastData = new Queue<GPSMessage>();
            Start();
        }

        public class ConnectionInfo
        {
            public Socket Socket { get; set; }
            public Thread Thread { get; set; }
        }

        private Thread _acceptThread;
        public List<ConnectionInfo> Connections =
            new List<ConnectionInfo>();

        public void Start()
        {
            SetupServerSocket();
            _acceptThread = new Thread(AcceptConnections);
            _acceptThread.IsBackground = true;
            _acceptThread.Start();
        }

        private void SetupServerSocket()
        {
            _serverSocket = new Socket(
                AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);
            _serverSocket.Bind(new IPEndPoint(IPAddress.Any, _port));
            _serverSocket.Listen((int)
                SocketOptionName.MaxConnections);
        }

        private void AcceptConnections()
        {
            while (true)
            {
                // Принимаем соединение
                var socket = _serverSocket.Accept();
                socket.ReceiveTimeout = _threadLife;
                var connection = new ConnectionInfo();
                connection.Socket = socket;
                // Создаем поток для получения данных
                connection.Thread = new Thread(ProcessConnection);
                connection.Thread.IsBackground = true;
                connection.Thread.Name = DateTime.Now.ToString();

                LogManager.GetCurrentClassLogger().Debug("start thread " + connection.Thread.Name);
                connection.Thread.Start(connection);

                // Сохраняем сокет
                lock (Connections) Connections.Add(connection);
            }
        }

        public bool AddToDB(GPSMessage msg)
        {
            lock (_lockIds)
            {
                if (msg.Speed <= 1)
                {
                    if (!IdsSpeed0.ContainsKey(msg.Id))
                        IdsSpeed0.Add(msg.Id, 0);
                    else
                    {
                        if (IdsSpeed0[msg.Id] >= MaxCountSpeed0)
                            return false;

                        IdsSpeed0[msg.Id]++;
                    }
                }
                else
                {
                    if (IdsSpeed0.ContainsKey(msg.Id))
                        IdsSpeed0[msg.Id] = 0;
                }
            }

            SqlExecute.Work(x => x.AddMessageInDB(msg));
            return true;
        }

        public bool AddToDB(string row)
        {
            var msg = GPSMessage.ParseRow(row);
            if (msg == null)
                return false;

            return AddToDB(msg);
        }

        private void ProcessConnection(object state)
        {
            ConnectionInfo connection = (ConnectionInfo)state;
            byte[] buffer = new byte[2048];
            try
            {
                while (true)
                {
                    int bytesRead = connection.Socket.Receive(buffer);
                    if (bytesRead > 0)
                    {
                        var message = Encoding.UTF8.GetString(buffer).Remove(bytesRead);
                        var messages = message.Split(spliter, StringSplitOptions.RemoveEmptyEntries);

                        for (int i = 0; i < messages.Length; i++)
                        {
                            message = messages[i].Trim();

                            var rows = message.Split(new[] { ')' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim() + ")");
                            foreach (var row in rows)
                            {
                                var msg = GPSMessage.ParseRow(row);
                                if (msg == null)
                                {
                                    if (!row.EndsWith("HSO)"))
                                        LogManager.GetCurrentClassLogger().Info("not added [" + row + "] length: " + row.Length);
                                    return;
                                }

                                lock (_queueLock)
                                {
                                    LastData.Enqueue(msg);
                                    if (LastData.Count > _maxQueue)
                                        LastData.Dequeue();

                                    AddToDB(msg);
                                }
                            }
                        }

                    }
                    else if (bytesRead == 0)
                        return;
                }
            }
            catch (SocketException exc)
            {
                if (exc.ErrorCode != 10060)
                {
                    var name = connection == null || connection.Thread == null ? string.Empty : connection.Thread.Name;
                    LogManager.GetCurrentClassLogger().Debug("Socket exception (thread " + name + "): " + exc.SocketErrorCode);
                }
            }
            catch (Exception exc)
            {
                var name = connection == null || connection.Thread == null ? string.Empty : connection.Thread.Name;
                LogManager.GetCurrentClassLogger().Debug("Exception (thread " + name + "): " + exc);
            }
            finally
            {
                LogManager.GetCurrentClassLogger().Debug("close thread " + connection.Thread.Name);
                connection.Socket.Close();
                lock (Connections) Connections.Remove(
                    connection);
            }
        }
    }
}