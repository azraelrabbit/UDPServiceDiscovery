using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Mono.Unix;
using Mono.Unix.Native;

namespace ServiceTest
{
    public class SignalHelper
    {

        public static event EventHandler<SingalEventArgs> OnSingalHandled; 

        public static  void StartHandleSingal()
        {

            Task.Run(() => WaitSingal());
        }

        private static void WaitSingal()
        {
            while (true)
            {
                try
                {
                    var ret = Mono.Unix.UnixSignal.WaitAny(new UnixSignal[]
                        {new UnixSignal(Signum.SIGILL), new UnixSignal(Signum.SIGTERM), new UnixSignal(Signum.SIGQUIT),new UnixSignal(Signum.SIGHUP)  });
                    
                    //process singal
                    if (ret > 0)
                    {
                        OnOnSingalHandled(new SingalEventArgs((Signum)ret));
                    }
                   
                }
                catch (Exception ex)
                {
                    //ignore
                }

            }
        }

        protected static  void OnOnSingalHandled(SingalEventArgs e)
        {
            Task.Run(()=>
            {
                try
                {
                    OnSingalHandled?.Invoke(null, e);
                }
                catch (Exception ex)
                {
                    //ignore
                }
            });
        }
    }

    public class SingalEventArgs:EventArgs
    {
        public Signum SigNum { get; set; }

        public SingalEventArgs(Signum sig)
        {
            SigNum = sig;
        }
    }
}
