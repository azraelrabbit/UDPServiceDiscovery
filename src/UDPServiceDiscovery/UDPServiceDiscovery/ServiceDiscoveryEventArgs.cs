using System;

namespace UDPServiceDiscovery
{
    public  class ServiceDiscoveryEventArgs : EventArgs
    {
        public byte[] MessageBuffer { get; set; }

        public ServiceDiscoveryEventArgs(byte[] buffer)
        {
            MessageBuffer = buffer;
        }
    }
}