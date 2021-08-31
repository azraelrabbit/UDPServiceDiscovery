using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace AZ.UDPServiceDiscovery.Messages
{
    public interface IMessage
    {
        public string Identity { get; set; }
 
    }
 
}
