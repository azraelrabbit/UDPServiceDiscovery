using System;
using System.IO;
 
 
using System.Threading.Tasks;
using Mono.Unix;
using Mono.Unix.Native;

namespace ServiceTest
{
    class Program
    {
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



            SignalHelper.OnSingalHandled += SignalHelper_OnSingalHandled;

            SignalHelper.StartHandleSingal();


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
