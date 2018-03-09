using ServiceStack;

namespace ExpressBase.ServerEvents.Services
{
    public class BaseService : Service
    {
        protected IServerEvents ServerEvents;

        public BaseService(IServerEvents _se)
        {
            this.ServerEvents = _se;
        }
    }
}