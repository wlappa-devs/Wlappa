using System;
using Shared.Protos;

namespace Server.Domain.ChainOfResponsibilityUtils
{
    public interface ISubscriptionManager<out T> where T : ClientMessage
    {
        public void SubscribeForClient(Guid clientId, IClientEventSubscriber<T> subscriber);
        public void UnsubscribeFromClient(Guid clientId);
    }
}