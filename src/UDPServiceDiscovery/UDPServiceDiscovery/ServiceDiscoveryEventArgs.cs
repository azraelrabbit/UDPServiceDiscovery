using System;
using System.Net;

namespace UDPServiceDiscovery
{
    public  class ServiceDiscoveryEventArgs : EventArgs
    {
        public byte[] MessageBuffer { get; private set; }
        public IPEndPoint RemoteEndPoint { get; private set; }

        public ServiceDiscoveryEventArgs(byte[] buffer, IPEndPoint remoteEndPoint)
        {
            MessageBuffer = buffer;
            RemoteEndPoint = remoteEndPoint;
        }
    }
}