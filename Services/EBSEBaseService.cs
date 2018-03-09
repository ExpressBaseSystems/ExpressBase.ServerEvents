using ServiceStack;

namespace ExpressBase.ServerEvents.Services
{
    public class EbSeBaseService : Service
    {
        protected IServerEvents ServerEvents;

        public EbSeBaseService(IServerEvents _se)
        {
            this.ServerEvents = _se;
        }
    }
}