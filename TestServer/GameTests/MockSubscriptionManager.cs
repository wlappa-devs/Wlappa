using System;
using Server.Domain.ChainOfResponsibilityUtils;
using Shared.Protos;

namespace TestServer.GameTests
{
    public class MockSubscriptionManager<T> : ISubscriptionManager<T> where T : InGameClientMessage
    {
        public IClientEventSubscriber<T>? Subscriber { get; private set; }

        public void SubscribeForClient(Guid clientId, IClientEventSubscriber<T> subscriber)
        {
            Subscriber = subscriber;
        }

        public void UnsubscribeFromClient(Guid clientId)
        {
        }
    }
}