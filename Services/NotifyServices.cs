﻿using ExpressBase.Objects.ServerEvents_Artifacts;
using ServiceStack;
using ServiceStack.Configuration;
using System;

namespace ExpressBase.ServerEvents.Services
{
    public class NotifyServices : BaseService
    {
        public NotifyServices(IServerEvents _se) : base(_se)
        {
        }

        public bool Any(NotifyUserIdRequest request)
        {
            try
            {
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
                Console.WriteLine(e.ToJson());
                return false;
            }
            return true;
        }

        public bool Any(NotifySubsribtionRequest request)
        {
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
                    return false;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToJson());
                return false;
            }
            return true;
        }

        public bool Any(NotifyChannelRequest request)
        {
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
                return false;
            }
            return true;
        }
    }
}
