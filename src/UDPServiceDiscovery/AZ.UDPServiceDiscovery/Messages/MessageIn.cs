using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace AZ.UDPServiceDiscovery.Messages
{
    public class ServiceDescMessage:IMessage
    {
        public string Identity { get; set; }
 
        public string ServiceTag { get; set; }

        public string ServiceName { get; set; }

        public string ServiceIPAddress { get; set; }

        public int Port { get; set; }
    }

    public class ServiceDescoverMessage : IMessage
    {
        public string Identity { get; set; }
 

        public string ServiceTag { get; set; }

        public string ServiceName { get; set; }

        public string ServiceIPAddress { get; set; }

        public int Port { get; set; }
    }
}
