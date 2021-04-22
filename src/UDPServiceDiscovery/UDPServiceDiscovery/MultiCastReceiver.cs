using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace UDPServiceDiscovery
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
                var nic = NetworkHelper.GetConnectedNetworkInterfaces().FirstOrDefault();

                var localaddr = nic.GetIPProperties().UnicastAddresses
                    .FirstOrDefault(p => p.Address.AddressFamily == AddressFamily.InterNetwork).Address;

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

            var receivedfuffer = client.EndReceive(ar, ref endpoint);


            //put out received buffer data

            //Console.WriteLine(Encoding.UTF8.GetString(receivedfuffer));


            OnOnDiscovery(new ServiceDiscoveryEventArgs(receivedfuffer));

            client.BeginReceive(OnReceived, client);

        }

        protected virtual void OnOnDiscovery(ServiceDiscoveryEventArgs e)
        {
            //Task.Run(() => OnDiscovery?.Invoke(this, e));

            //OnDiscovery?.BeginInvoke(this, e, null, null);
            //OnDiscovery?.Invoke(this, e);

            if (OnDiscovery != null)
            {
                //var tasks = OnDiscovery.GetInvocationList().Cast<EventHandler<ServiceDiscoveryEventArgs>>().Select(s => s(this, e));

                var ls = OnDiscovery.GetInvocationList().Cast<EventHandler<ServiceDiscoveryEventArgs>>();//.Where(p => p.Invoke(this, e));

                foreach (var ev in ls)
                {
                    ev.Invoke(this, e);
                }


                //await Task.WhenAll(tasks);
            }

        }



    }
}
