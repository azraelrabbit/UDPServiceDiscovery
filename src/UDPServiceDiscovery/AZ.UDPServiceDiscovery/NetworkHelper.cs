using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace AZ.UDPServiceDiscovery
{
    public static class NetworkHelper
    {
        public static short TTL = 255;

        private static readonly List<string> VirtualNetworkKeyWords = new List<string>() { "虚拟", "vm", "virtual", "vpn", "software", "wan", "bluetooth", "teredo" };
        /// <summary>
        /// 
        /// </summary>
        ///// <returns></returns>
        public static List<NetworkInterface> GetLocalInterfaceList()
        {
            var list = NetworkInterface.GetAllNetworkInterfaces()
                .ToList();

 
            //var locallistStr = string.Join("|", list.Select(p => p.Description).ToList());
            //Console.WriteLine(locallistStr);
            return list.FindAll(p => !VirtualNetworkKeyWords.Exists(v => p.Description.ToLower().Contains(v)));
        }

        public static List<NetworkInterface> GetConnectedNetworkInterfaces()
        {
            var niclist= GetLocalInterfaceList().Where(p => p.OperationalStatus == OperationalStatus.Up &&p.NetworkInterfaceType!=NetworkInterfaceType.Loopback).ToList();
            
            
            //var retlist=niclist.Where(p=>p)
            return niclist;
        }

        public static IPAddress GetLocalIpAddress()
        {
            var nic = NetworkHelper.GetConnectedNetworkInterfaces().FirstOrDefault();

            var localaddr = nic.GetIPProperties().UnicastAddresses
                .FirstOrDefault(p => p.Address.AddressFamily == AddressFamily.InterNetwork).Address;
            return localaddr;
        }
    }
}
