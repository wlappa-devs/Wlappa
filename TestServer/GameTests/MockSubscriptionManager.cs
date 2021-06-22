using System;
using Server.Application.ChainOfResponsibilityUtils;
using Shared.Protos;

namespace TestServer.GameTests
{
    public class MockSubscriptionManager<T>: ISubscriptionManager<T> where T: InGameClientMessage
    {
        public void SubscribeForClient(Guid clientId, IClientEventSubscriber<T> subscriber)
        {
            Console.WriteLine("Subscription happened");
        }

        public void UnsubscribeFromClient(Guid clientId)
        {
            Console.WriteLine("Unsubscription happened");
        }
    }
}