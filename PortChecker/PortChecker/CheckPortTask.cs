using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Windows.Forms;
using System.Diagnostics;

namespace MatthiWare.PortChecker
{
    
    public class PortChecker
    {
        private delegate void StartDelegate();

        public event EventHandler<PortEventArgs> PortCheckCompleted;
        internal bool _running = false;
        public UInt16 MinimumPort { get; set; }
        public UInt16 MaximumPort { get; set; }
        public IPAddress Ip { get; set; }

        public PortChecker(UInt16 minPort, UInt16 maxPort, IPAddress ip)
        {
            MinimumPort = minPort;
            MaximumPort = maxPort;
            Ip = ip;
        }

        public void StartChecking()
        {
            _running = true;
            StartDelegate sd = new StartDelegate(Start);
            sd.BeginInvoke((ar) =>
            {
                Debug.WriteLine("All tasks started!");
            }, null);

        }

        private void Start()
        {
            for (UInt16 i = MinimumPort; i <= MaximumPort; i++)
            {
                if (!_running)
                    return;

                CheckPortTask task = new CheckPortTask(PortCheckCompleted, Ip);
                task.BeginCheckPort(i);
            }

            _running = false;
        }

        public void End()
        {
            _running = false;
        }
    }

    public class CheckPortTask
    {
        private EventHandler<PortEventArgs> _event;
        private IPAddress _ip;

        public CheckPortTask(EventHandler<PortEventArgs> Event, IPAddress ip)
        {
            _event = Event;
            _ip = ip;
        }

        private delegate bool CheckPort(UInt16 port);

        public IAsyncResult BeginCheckPort(UInt16 portToCheck)
        {
            TcpClient tcpClient = new TcpClient();
            AsyncState asyncState = new AsyncState(portToCheck, tcpClient);
            return tcpClient.BeginConnect(_ip, portToCheck, new AsyncCallback(EndCheckPort), asyncState);
        }

        public void EndCheckPort(IAsyncResult ar)
        {
            AsyncState asyncState = ar.AsyncState as AsyncState;
            TcpClient client = asyncState.Client;
            UInt16 port = asyncState.Port;
            bool connected = client.Connected;

            try
            {
                client.EndConnect(ar);

                if (_event != null)
                    _event(this, new PortEventArgs(port, connected, (connected) ? "Succesfully connected!" : "Unable to connect!"));
            }
            catch (Exception e)
            {
                if (_event != null)
                    _event(this, new PortEventArgs(port, false, e.Message));
            }
        }
    }

    public class PortEventArgs : EventArgs
    {
        public UInt16 Port { get; set; }
        public Boolean Open { get; set; }
        public String Message { get; set; }

        public PortEventArgs(UInt16 port, Boolean open, String message)
            : base() 
        {
            Port = port;
            Open = open;
            Message = message;
        }
    }
}
