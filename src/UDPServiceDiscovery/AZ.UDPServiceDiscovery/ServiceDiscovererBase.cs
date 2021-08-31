using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using AZ.UDPServiceDiscovery.Messages;
using Newtonsoft.Json;

namespace AZ.UDPServiceDiscovery
{
    public class ServiceDiscovererBase<TIn, TOut> : IDisposable where TIn : IMessage where TOut : IMessage
    {
        public byte[] Message { get; private set; }
        private static readonly IPAddress _DefaulIpAddress = IPAddress.Parse("239.255.255.101");
        private static readonly IPEndPoint _defaultEndpoint = new IPEndPoint(_DefaulIpAddress, 12678);

        private readonly IPEndPoint _multiCastEndPoint;

        //public string ServiceTag { get; private set; }
        //public string ServiceName { get; private set; }
        //public IPEndPoint ServiceEndPoint { get; private set; }

        private MultiCastSender _sender;

        private MultiCastReceiver _receiver;

        protected string Identity { get; private set; }


        private bool _disposed = false;


        public event EventHandler<DiscoveringEventArgs<TOut>> OnDiscovering;

        public ServiceDiscovererBase(string idendity, TIn message, IPEndPoint multiCastEndPoint = null)
        {
            Identity = message.Identity;
            Message = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
            //ServiceTag = serviceTag;
            //ServiceName = serviceName;
            //ServiceEndPoint = serviceEndPoint;

            if (multiCastEndPoint == null)
            {
                _multiCastEndPoint = _defaultEndpoint;
            }
            else
            {
                _multiCastEndPoint = multiCastEndPoint;
            }

            _sender = new MultiCastSender(_multiCastEndPoint.Address.ToString(), _multiCastEndPoint.Port);

            _receiver = new MultiCastReceiver(_multiCastEndPoint.Address.ToString(), _multiCastEndPoint.Port);

            _receiver.OnDiscovery += _receiver_OnDiscovery;

           
        }

        public void Send(TIn message)
        {
            var msgBuf = JsonConvert.SerializeObject(message);
            _sender.Send(msgBuf);
        }

        public void Start(int interval=5000)
        {

            _sender.Start(Message,interval);

            _receiver.Start();
        }

        private void _receiver_OnDiscovery(object sender, ServiceDiscoveryEventArgs e)
        {
            DoReceiveMessage(e);
        }

        public virtual void DoReceiveMessage(ServiceDiscoveryEventArgs e)
        {
            if (OnDiscovering != null)
            {
                var ls = OnDiscovering.GetInvocationList()
                    .Cast<EventHandler<DiscoveringEventArgs<TOut>>>(); //.Where(p => p.Invoke(this, e));

                var msgOutArgs = new DiscoveringEventArgs<TOut>(e.MessageBuffer, e.RemoteEndPoint);

                if (msgOutArgs.Message.Identity == Identity)
                {
                    //this message from this self process.
                }
                else
                {
                    foreach (var ev in ls)
                    {
                        ev.Invoke(this, msgOutArgs);
                    }
                }
            }
        }

        public void Dispose()
        {

            if (_disposed)
            {
                return;
            }
            _disposed = true;
            if (_receiver != null)
            {
                _receiver.OnDiscovery -= _receiver_OnDiscovery;
            }
           

            _sender?.Stop();
            _receiver?.Stop();

            //_sender?.Dispose();
            //_receiver?.Dispose();

            _receiver = null;
            _sender = null;
        }
    }
}
