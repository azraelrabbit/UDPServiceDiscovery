﻿using System;
//using System.ServiceProcess;
using UDPServiceDiscovery;

namespace TestServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var serer = new MultiCastSender();


            var timeSpan = 5000;

            Console.WriteLine("inpute your mesage and press enter key:");
            var message=Console.ReadLine()+"| from : "+Environment.MachineName;

            if (string.IsNullOrEmpty(message))
            {
                message = "Empty Message, from: "+Environment.MachineName;
            }

            serer.Start(message,timeSpan);    
            


            Console.WriteLine("your message sending to client every {0} seconds. presss enter key to stop.",timeSpan);


            Console.ReadLine();

            serer.Stop();
      

            Console.WriteLine("Resources disposed. press enter key to exit.");
            Console.ReadLine();
        }
    }

    
}
