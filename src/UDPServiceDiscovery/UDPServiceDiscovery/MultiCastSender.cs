using System;
using System.Collections.Generic;
using System.Linq;
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

        public MultiCastSender(string multiCastGroupIpAddr = "239.255.255.101", int port = 12678, bool useAny = false)
        {
            _multicastGroup = multiCastGroupIpAddr;
            _multicastPort = port;
            this.Ttl = NetworkHelper.TTL;
            
            //reuse port
            this.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
 
            this.Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, NetworkHelper.TTL);


            var groupAddress = IPAddress.Parse(_multicastGroup);
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



            this.EnableBroadcast = false;

  
            _multiCastEndpoint = new IPEndPoint(groupAddress, _multicastPort);

 
            var localIpEnd = new IPEndPoint(IPAddress.Any, _multicastPort);


            this.JoinMulticastGroup(groupAddress,localAddress);
       
            this.Client.Bind(localIpEnd);
        }


        public void Start(string message,int interval=5000)
        {

            //this.message = message;
            //_timer = new Timer(OntimerCallback, null, 0, 5000);
            _timer = new Timer(OntimerCallback,null,TimeSpan.Zero,TimeSpan.FromMilliseconds(interval));
            //_timer.Change(0, 5000);
            _timer.Change(TimeSpan.Zero, TimeSpan.FromMilliseconds(interval));

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

            var groupAddress = IPAddress.Parse(_multicastGroup);
            this.DropMulticastGroup(groupAddress);

            this.Dispose();
 
        }
    }
}
