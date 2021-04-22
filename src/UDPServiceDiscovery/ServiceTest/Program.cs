using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using Mono.Unix;
using Mono.Unix.Native;

namespace ServiceTest
{
    class Program
    {
        private static readonly List<string> VirtualNetworkKeyWords = new List<string>() { "虚拟", "vm", "virtual", "vpn", "software", "wan", "bluetooth", "teredo" };
        /// <summary>
        /// 
        /// </summary>
        ///// <returns></returns>
        public static List<NetworkInterface> GetLocalInterfaceList()
        {
            var list = NetworkInterface.GetAllNetworkInterfaces()
                .ToList();


            var locallistStr = string.Join("|", list.Select(p => p.Description).ToList());
            Console.WriteLine(locallistStr);
            return list.FindAll(p => !VirtualNetworkKeyWords.Exists(v => p.Description.ToLower().Contains(v)));
        }


        static void Main(string[] args)
        {

            //Application
 
            Console.WriteLine("Hello World!");

            //var fileName = Path.Combine(PlatformServices.Default.Application.ApplicationBasePath, "log.txt");
            //ServiceRunner<ExampleService>.Run(config =>
            //{
            //    var name = config.GetDefaultName();
            //    config.Service(serviceConfig =>
            //    {
            //        serviceConfig.ServiceFactory((extraArguments, controller) =>
            //        {
            //            return new ExampleService(controller);
            //        });

            //        serviceConfig.OnStart((service, extraParams) =>
            //        {
            //            Console.WriteLine("Service {0} started", name);
            //            service.Start();
            //        });

            //        serviceConfig.OnStop(service =>
            //        {
            //            Console.WriteLine("Service {0} stopped", name);
            //            service.Stop();
            //        });

            //        serviceConfig.OnError(e =>
            //        {
            //            File.AppendAllText(fileName, $"Exception: {e.ToString()}\n");
            //            Console.WriteLine("Service {0} errored with exception : {1}", name, e.Message);
            //        });
            //    });
            //});


            //var sm = NetworkInterface.GetAllNetworkInterfaces().Where(p => p.SupportsMulticast);

            //if (sm.Any())
            //{
            //    //foreach (var ni in sm)
            //    //{
            //    //    Console.WriteLine(ni.Name+ni.Description+ni.GetIPv4Statistics().ToString());

            //    //}

            //    IPGlobalProperties computerProperties = IPGlobalProperties.GetIPGlobalProperties();
            //    //NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
            //    Console.WriteLine("Interface information for {0}.{1}     ",
            //        computerProperties.HostName, computerProperties.DomainName);
            //    foreach (NetworkInterface adapter in sm)
            //    {
            //Console.WriteLine(" Interface name .......................... : {0}", adapter.Name);
            //        IPInterfaceProperties properties = adapter.GetIPProperties();
            //        Console.WriteLine(adapter.Description);
            //        Console.WriteLine(String.Empty.PadLeft(adapter.Description.Length, '='));
            //        Console.WriteLine("  Interface type .......................... : {0}", adapter.NetworkInterfaceType);
            //        Console.WriteLine("  Physical Address ........................ : {0}",
            //            adapter.GetPhysicalAddress().ToString());
            //        Console.WriteLine("  Is receive only.......................... : {0}", adapter.IsReceiveOnly);
            //        Console.WriteLine("  Multicast................................ : {0}", adapter.SupportsMulticast);
            //Console.WriteLine("  IS UP    ................................ : {0}", adapter.OperationalStatus);
            //        Console.WriteLine();
            //    }
            //}

            var sm = GetLocalInterfaceList();

            IPGlobalProperties computerProperties = IPGlobalProperties.GetIPGlobalProperties();
            //NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
            Console.WriteLine("Interface information for {0}.{1}     ",
                computerProperties.HostName, computerProperties.DomainName);
            foreach (NetworkInterface adapter in sm)
            {
                //AddressFamily.InterNetwork
                var localIpv4 = adapter.GetIPProperties().UnicastAddresses.FirstOrDefault(p=>p.Address.AddressFamily==AddressFamily.InterNetwork);
                var localIpv6 = adapter.GetIPProperties().UnicastAddresses.FirstOrDefault(p => p.Address.AddressFamily == AddressFamily.InterNetworkV6);


                Console.WriteLine( " Interface name .......................... : {0}", adapter.Name);
                IPInterfaceProperties properties = adapter.GetIPProperties();
                Console.WriteLine(adapter.Description);
                Console.WriteLine(String.Empty.PadLeft(adapter.Description.Length, '='));
                Console.WriteLine("  Interface type .......................... : {0}", adapter.NetworkInterfaceType);
                Console.WriteLine("  Physical Address ........................ : {0}",
                    adapter.GetPhysicalAddress().ToString());
                Console.WriteLine("  Is receive only.......................... : {0}", adapter.IsReceiveOnly);
                Console.WriteLine("  Multicast................................ : {0}", adapter.SupportsMulticast);
                Console.WriteLine("  IS UP    ................................ : {0}", adapter.OperationalStatus);

                Console.WriteLine("  IPv4     ................................ : {0}", localIpv4.Address.ToString());
                Console.WriteLine("  IPv4     ................................ : {0}", localIpv6.Address.ToString());
                Console.WriteLine();
            }

            //SignalHelper.OnSingalHandled += SignalHelper_OnSingalHandled;

            //SignalHelper.StartHandleSingal();


            Console.ReadLine();
        }

        private static void SignalHelper_OnSingalHandled(object sender, SingalEventArgs e)
        {
            System.Console.WriteLine(e.SigNum);
            switch (e.SigNum)
            {
                case Signum.SIGKILL:
                    Console.WriteLine("killing me");
                    WaitPress();
                    break;
                case Signum.SIGTERM:
                    Console.WriteLine("term me");
                    WaitPress();
                    break;
                case Signum.SIGQUIT:
                    Console.WriteLine("termnal quit");
                    WaitPress();
                    break;
                case Signum.SIGHUP:
                    Console.WriteLine("hungup me.");
                    WaitPress(() => { Environment.Exit((int)e.SigNum); });
                    break;
                default:
                        Console.WriteLine("default,do nothing");
                        break;
            }
        }

        static void WaitPress(Action callback=null)
        {
            Console.WriteLine("press enter key to continue");

          
            Console.ReadLine();
            callback?.Invoke();
        }
    }
}
