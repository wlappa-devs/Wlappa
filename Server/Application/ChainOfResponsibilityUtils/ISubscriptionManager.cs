using System;
using Shared.Protos;

namespace Server.Application.ChainOfResponsibilityUtils
{
    public interface ISubscriptionManager<T> where T : ClientMessage
    {
        public void SubscribeForClient(Guid clientId, IClientEventSubscriber<T> subscriber);
        public void UnsubscribeFromClient(Guid clientId);
    }
}