using ServiceStack;
using ServiceStack.Messaging;
using ServiceStack.RabbitMq;

namespace ExpressBase.ServerEvents.Services
{
    public class BaseService : Service
    {
        protected IServerEvents ServerEvents;

        protected RabbitMqProducer MessageProducer3 { get; private set; }

        public BaseService(IServerEvents _se)
        {
            this.ServerEvents = _se;
        }

        public BaseService(IServerEvents _se, IMessageProducer _mqp)
        {
            this.ServerEvents = _se;
            this.MessageProducer3 = _mqp as RabbitMqProducer;
        }
    }
}