using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace UDPServiceDiscovery
{
    public class MultiCastReceiver:UdpClient
    {
        private string _multicastGroup = "239.255.255.101";

        private readonly int _multicastPort;

        private bool _disposed;

        public event EventHandler<ServiceDiscoveryEventArgs> OnDiscovery;

        public MultiCastReceiver(int port=12678)
        {
            _multicastPort = port;
            var groupAddress = IPAddress.Parse(_multicastGroup);
            this.JoinMulticastGroup(groupAddress);
            this.EnableBroadcast = false;
          
            var localIpEnd=new IPEndPoint(IPAddress.Any, _multicastPort);

            //reuse port
            this.Client.SetSocketOption(SocketOptionLevel.Socket,SocketOptionName.ReuseAddress,true);
            this.Client.Bind(localIpEnd);

        }

        public void Start()
        { 
            this.BeginReceive(OnReceived, this);
        }

        public void Stop()
        {
            _disposed = true;
            this.Dispose();
        }


        private void OnReceived(IAsyncResult ar)
        {
            if (_disposed)
            {
                return;
            }
            var client = (UdpClient)ar.AsyncState;

            var endpoint =(IPEndPoint) client.Client.LocalEndPoint;

            var receivedfuffer = client.EndReceive(ar, ref endpoint);

            //put out received buffer data

            //Console.WriteLine(Encoding.UTF8.GetString(receivedfuffer));

            OnOnDiscovery(new ServiceDiscoveryEventArgs(receivedfuffer));

              client.BeginReceive(OnReceived, client);

        }

        protected virtual void OnOnDiscovery(ServiceDiscoveryEventArgs e)
        {
            Task.Run(() => OnDiscovery?.Invoke(this, e));
        }
    }
}
