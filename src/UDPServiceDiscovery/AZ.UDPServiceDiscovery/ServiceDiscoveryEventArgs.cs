using Newtonsoft.Json;
using System;
using System.Net;
using System.Text;
using AZ.UDPServiceDiscovery.Messages;

namespace AZ.UDPServiceDiscovery
{
    public  class ServiceDiscoveryEventArgs : EventArgs
    {
        public byte[] MessageBuffer { get; private set; }
        public IPEndPoint RemoteEndPoint { get; private set; }

        public ServiceDiscoveryEventArgs(byte[] buffer,IPEndPoint remoteEndPoint)
        {
            MessageBuffer = buffer;
            RemoteEndPoint = remoteEndPoint;
        }
    }

    public class DiscoveringEventArgs <T> : EventArgs where T:IMessage
    {
        public byte[] MessageBuffer { get; private set; }
        public IPEndPoint RemoteEndPoint { get; private set; }

        public T Message { get; set; }

        public DiscoveringEventArgs(byte[] buffer, IPEndPoint remoteEndPoint)
        {
            MessageBuffer = buffer;
            RemoteEndPoint = remoteEndPoint;
            Message = JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(buffer));
          
        }
    }
}