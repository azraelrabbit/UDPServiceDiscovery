using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace AZ.UDPServiceDiscovery
{
    public class MultiCastReceiver : UdpClient
    {
        private string _multicastGroup = "239.255.255.101";

        private readonly int _multicastPort;

        private bool _disposed;

        public event EventHandler<ServiceDiscoveryEventArgs> OnDiscovery;

        public MultiCastReceiver(string multiCastGroupIpAddr = "239.255.255.101", int port = 12678)
        {
            _multicastGroup = multiCastGroupIpAddr;
            _multicastPort = port;
            var groupAddress = IPAddress.Parse(_multicastGroup);
 
            this.EnableBroadcast = false;
 
            var localAddress = IPAddress.Any;

            if (Environment.OSVersion.Platform == PlatformID.Win32NT || Environment.OSVersion.Platform == PlatformID.Win32S || Environment.OSVersion.Platform == PlatformID.Win32Windows || Environment.OSVersion.Platform == PlatformID.WinCE)
            {
                var localaddr = NetworkHelper.GetLocalIpAddress();

                if (localaddr != null)
                {
                    Console.WriteLine("local addr :" + localaddr.ToString());
                    localAddress = localaddr;
                }

            }
            else
            {
                Console.WriteLine("local addr : 0.0.0.0");
            }

            var localIpEnd = new IPEndPoint(localAddress, _multicastPort);

            //reuse port
            this.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            this.Client.Ttl = NetworkHelper.TTL;
            this.Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, NetworkHelper.TTL);
            //this.Client.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.MulticastTimeToLive, 128);

            this.JoinMulticastGroup(groupAddress, localAddress);

            this.Client.Bind(localIpEnd);


        }

        public void Start()
        {
            this.BeginReceive(OnReceived, this);
        }

        public void Stop()
        {
            if (_disposed)
            {
                return;
            }
            _disposed = true;


            var groupAddress = IPAddress.Parse(_multicastGroup);
            this.DropMulticastGroup(groupAddress);
            this.Dispose();
        }


        private void OnReceived(IAsyncResult ar)
        {
            if (_disposed)
            {
                return;
            }
            var client = (UdpClient)ar.AsyncState;

            var endpoint = (IPEndPoint)client.Client.LocalEndPoint;

            var remoteEndpoint = client.Client.RemoteEndPoint;

            var remoteIpEndpoint = (IPEndPoint) remoteEndpoint;

            var receivedfuffer = client.EndReceive(ar, ref endpoint);


            //put out received buffer data
 
            OnOnDiscovery(new ServiceDiscoveryEventArgs(receivedfuffer, remoteIpEndpoint));

            client.BeginReceive(OnReceived, client);

        }

        protected virtual void OnOnDiscovery(ServiceDiscoveryEventArgs e)
        {
 
            if (OnDiscovery != null)
            {
 
                var ls = OnDiscovery.GetInvocationList().Cast<EventHandler<ServiceDiscoveryEventArgs>>();//.Where(p => p.Invoke(this, e));

                foreach (var ev in ls)
                {
                    ev.Invoke(this, e);
                }
 
            }

        }

      

    }
}
