using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using AsyncIO;

namespace GPSTrackerService
{
    public class TrackerServerAIO
    {
        public TrackerServerAIO(int port)
        {
            var completionPort = CompletionPort.Create();

            var listenerEvent = new AutoResetEvent(false);
            var serverEvent = new AutoResetEvent(false);

            var listener = AsyncSocket.Create(AddressFamily.InterNetwork,SocketType.Stream, ProtocolType.Tcp);
            completionPort.AssociateSocket(listener, listenerEvent);

            AsyncSocket server = AsyncSocket.Create(AddressFamily.InterNetwork,SocketType.Stream, ProtocolType.Tcp);
            completionPort.AssociateSocket(server, serverEvent);

            Task.Factory.StartNew(() =>
            {
                CompletionStatus completionStatus;

                while (true)
                {
                    var result = completionPort.GetQueuedCompletionStatus(-1, out completionStatus);

                    if (result)
                    {
                        Console.WriteLine("{0} {1} {2}", completionStatus.SocketError,
                            completionStatus.OperationType, completionStatus.BytesTransferred);

                        if (completionStatus.State != null)
                        {
                            var resetEvent = (AutoResetEvent)completionStatus.State;
                            resetEvent.Set();
                        }
                    }
                }
            });

            listener.Bind(IPAddress.Any, port);
            listener.Listen(1);
            
            listener.Accept(server);
            listenerEvent.WaitOne();

            byte[] sendBuffer = new byte[1] { 2 };
            byte[] recvBuffer = new byte[1];

            server.Receive(recvBuffer);

            serverEvent.WaitOne();

            server.Dispose();
            
        }
    }
}