using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace UDPServiceDiscovery
{
    public class MultiCastSender : UdpClient
    {
        private string _multicastGroup = "239.255.255.101";

        private readonly int _multicastPort;

        private readonly IPEndPoint _multiCastEndpoint;

        //private string message;

        private byte[] _messageBuffer;

        private System.Threading.Timer _timer;

        private bool _disposed;

        public MultiCastSender(int port=12678)
        {
            _multicastPort = port;

            var ipaddr = IPAddress.Parse(_multicastGroup);
            this.JoinMulticastGroup(ipaddr);
            this.EnableBroadcast = false;
            
            _multiCastEndpoint = new IPEndPoint(ipaddr, _multicastPort);
        }


        public void Start(string message,int interval=5000)
        {

            //this.message = message;
            _timer = new Timer(OntimerCallback, null, 0, 5000);
            _timer.Change(0, 5000);

            _messageBuffer = Encoding.UTF8.GetBytes(message);

        }

        private void OntimerCallback(object state)
        {
            if (_disposed)
            {
                return;
            }
            this.Send(_messageBuffer, _messageBuffer.Length, _multiCastEndpoint);
        }


        public void Stop()
        {
            _disposed = true;
            _timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);

            this.Dispose();
 
        }
    }
}
