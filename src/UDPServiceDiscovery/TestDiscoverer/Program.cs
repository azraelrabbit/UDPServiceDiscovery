using System;
using System.Net;
using AZ.UDPServiceDiscovery;
using AZ.UDPServiceDiscovery.Messages;

namespace TestDiscoverer
{
    class Program
    {
        private static ServiceDiscoverer discover;
        static void Main(string[] args)
        {
            Console.CancelKeyPress += Console_CancelKeyPress;
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
            var identity = Guid.NewGuid().ToString("x");

        typenm:

            Console.WriteLine("Please Enter your Service Name then press enter : ");
            var serviceName = Console.ReadLine();

            if (string.IsNullOrEmpty(serviceName))
            {
                Console.WriteLine("Service Name could not be null or empty!");

                goto typenm;
            }

            var sendMsg = new ServiceDescMessage()
            {
                Identity = identity,
                ServiceIPAddress = NetworkHelper.GetLocalIpAddress().ToString(),
                Port = 3344,
                ServiceName = serviceName,//"Cache Service",
                ServiceTag = "ZP"

            };

              discover = new ServiceDiscoverer(identity, sendMsg);

            discover.OnDiscovering += Discover_OnDiscovering;

            discover.Start(2000);


            Console.WriteLine("Press Enter Exit");
            Console.ReadLine();
            discover?.Dispose();

        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            Console.WriteLine("Process Cancel");
            // Console.ReadLine();

            discover?.Dispose();
            e.Cancel = true;

        }

        private static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            Console.WriteLine("Process terminating");
            // Console.ReadLine();

            discover?.Dispose();


        }

        private static void Discover_OnDiscovering(object sender, DiscoveringEventArgs<ServiceDescoverMessage> e)
        {
            var msg = $"{DateTime.Now.ToString()}|Get BroadCast : Service: {e.Message.ServiceName}, EndPoint:{e.Message.ServiceIPAddress}";

            Console.WriteLine(msg);
        }
    }
}
