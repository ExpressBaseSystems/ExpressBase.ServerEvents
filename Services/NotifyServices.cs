using ExpressBase.Common.ServerEvents_Artifacts;
using ServiceStack;
using ServiceStack.Messaging;
using System;
using System.Collections.Generic;

namespace ExpressBase.ServerEvents.Services
{
    public class NotifyServices : BaseService
    {
        public NotifyServices(IServerEvents _se, IMessageProducer _mqp) : base(_se, _mqp) { }

        [Authenticate]
        public NotifyResponse Post(NotifyUserIdRequest request)
        {
            Console.WriteLine("---------------------->Inside notifyUserIdReq \nUserId: " + request.ToUserAuthId);
            NotifyResponse res = new NotifyResponse();
            try
            {
                if (request.Selector == "cmd.onNotification")
                {
                    this.MessageProducer3.Publish(new NotificationToDBRequest()
                    {
                        SolnId = request.SolnId,
                        UserId = request.UserId,
                        NotifyUserId = request.NotifyUserId,
                        Notification = request.Msg,
                        NotificationId = request.NotificationId
                    });
                    Console.WriteLine("---------------------->After NotificationToDBRequest UserId: " + request.ToUserAuthId);
                }
                Console.WriteLine("request.Selector" + request.Selector + "------------No NotificationToDBRequest----------> UserId: " + request.ToUserAuthId);
                var subscriptionInfos = ServerEvents.GetSubscriptionInfosByUserId(request.ToUserAuthId);

                foreach (var sub in subscriptionInfos)
                    if (request.ToChannel == null)
                        ServerEvents.NotifySubscription(sub.SubscriptionId, request.Selector, request.Msg);
                    else
                        foreach (var channel in request.ToChannel)
                            ServerEvents.NotifySubscription(sub.SubscriptionId, request.Selector, request.Msg, channel);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + e.StackTrace);
                res.ResponseStatus.Message = e.Message;
            }
            return res;
        }

        [Authenticate]
        public NotifyResponse Post(NotifySubscriptionRequest request)
        {
            NotifyResponse res = new NotifyResponse();
            try
            {
                var subscriptionInfos = ServerEvents.GetSubscriptionInfo(request.ToSubId);

                if (!String.IsNullOrEmpty(subscriptionInfos.SubscriptionId))
                {
                    if (request.ToChannel == null)
                        ServerEvents.NotifySubscription(subscriptionInfos.SubscriptionId, request.Selector, request.Msg);
                    else
                        foreach (var channel in request.ToChannel)
                            ServerEvents.NotifySubscription(subscriptionInfos.SubscriptionId, request.Selector, request.Msg, channel);
                }
                else
                {
                    Console.WriteLine("SubscribtionId doesn't Exist");
                    res.ResponseStatus.Message = "SubscribtionId doesn't Exist";
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToJson());
                res.ResponseStatus.Message = e.Message;
            }
            return res;
        }

        [Authenticate]
        public NotifyResponse Post(NotifyChannelRequest request)
        {
            NotifyResponse res = new NotifyResponse();

            try
            {
                //var subscriptionInfos = ServerEvents.GetSubscriptionsDetails(request.ToChannel);

                //if (subscriptionInfos != null)
                //{
                foreach (var channel in request.ToChannel)
                    ServerEvents.NotifyChannel(channel, request.Selector, request.Msg);
                //}
                //    else
                //    {
                //    Console.WriteLine("No one listening in the Channel: " + request.ToChannel.ToJson());
                //}
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToJson());
                res.ResponseStatus.Message = e.Message;
            }
            return res;
        }

        [Authenticate]
        public NotifyResponse Post(NotifyUsersRequest request)
        {
            NotifyResponse res = new NotifyResponse();
            try
            {
                foreach (KeyValuePair<int, string> user_details in request.UsersDetails)
                {
                    this.Post(new NotifyUserIdRequest
                    {
                        Msg = request.Msg,
                        Selector = request.Selector,
                        ToUserAuthId = user_details.Value,
                        NotificationId = request.NotificationId,
                        NotifyUserId = user_details.Key,
                        SolnId = request.SolnId
                    });
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToJson());
                res.ResponseStatus.Message = e.Message;
            }
            return res;
        }
    }
}