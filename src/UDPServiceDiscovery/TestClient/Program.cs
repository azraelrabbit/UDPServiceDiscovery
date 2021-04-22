using System;
using System.Text;
using UDPServiceDiscovery;

namespace TestClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var receiver = new MultiCastReceiver();

            Console.WriteLine( " start receive multicast ");

            //Console.ReadLine();


            receiver.OnDiscovery += Receiver_OnDiscovery;
            receiver.Start();


            Console.WriteLine("begining receive. press enter key to stop listen");

            Console.ReadLine();

            receiver.OnDiscovery -= Receiver_OnDiscovery;
            receiver.Stop();


            Console.WriteLine("Resources disposed. press enter key to exit.");
            Console.ReadLine();
        }

        private static void Receiver_OnDiscovery(object sender, ServiceDiscoveryEventArgs e)
        {
            Console.WriteLine("i got : "+Encoding.UTF8.GetString(e.MessageBuffer)+" | "+DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff"));
        }
    }
}
