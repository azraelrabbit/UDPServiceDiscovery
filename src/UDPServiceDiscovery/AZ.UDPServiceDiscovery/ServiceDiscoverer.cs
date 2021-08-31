using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using AZ.UDPServiceDiscovery.Messages;

namespace AZ.UDPServiceDiscovery
{
    public class ServiceDiscoverer:ServiceDiscovererBase<ServiceDescMessage,ServiceDescoverMessage>
    {
        public ServiceDiscoverer(string identity,ServiceDescMessage message, IPEndPoint multiCastEndPoint = null) : base(identity,message, multiCastEndPoint)
        {
        }
    }
}
