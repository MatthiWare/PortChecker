using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;

namespace MatthiWare.PortChecker
{
    public class AsyncState
    {
        public UInt16 Port { get; set; }
        public TcpClient Client { get; set; }

        public AsyncState(UInt16 port, TcpClient client)
        {
            Client = client;
            Port = port;
        }
    }
}
