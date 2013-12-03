using System;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;

namespace Gateway
{
    public class MessageListener
    {
        public event EventHandler<ToggleCommand> On;

        protected virtual void InvokeOn(ToggleCommand e)
        {
            EventHandler<ToggleCommand> handler = On;
            if (handler != null) handler(this, e);
        }

        private MessageReceiver messageReceiver;
        public MessageListener(MessageReceiver receiver)
        {
            messageReceiver = receiver;
        }

        public void Listen()
        {
            while (!_shouldStop)
            {
                try
                {
                    BrokeredMessage message = messageReceiver.Receive(new TimeSpan(0, 0, 10));
                    if (message != null)
                    {
                       var body = message.GetBody<string>();
                       InvokeOn(JsonConvert.DeserializeObject<ToggleCommand>(body));
                       
                        message.Complete();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Caught exception: " + e.Message);
                    break;
                }
            }
        }

        public void RequestStop()
        {
            _shouldStop = true;
        }

        private volatile bool _shouldStop;

    }
}