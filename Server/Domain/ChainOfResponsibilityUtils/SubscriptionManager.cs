using System;
using Shared.Protos;

namespace Server.Domain.ChainOfResponsibilityUtils
{
    public class SubscriptionManager<T> : ISubscriptionManager<T> where T : ClientMessage
    {
        private readonly IClientEventEmitterResolver<T> _resolver;

        public SubscriptionManager(IClientEventEmitterResolver<T> resolver)
        {
            _resolver = resolver;
        }

        public void SubscribeForClient(Guid clientId, IClientEventSubscriber<T> subscriber)
        {
            var client = _resolver.Resolve(clientId);
            if (client is null) throw new InvalidOperationException("Can't find emitter for client");
            client.EventListener = subscriber.EventHandle;
        }

        public void UnsubscribeFromClient(Guid clientId)
        {
            var client = _resolver.Resolve(clientId);
            if (client is null) return;
            client.EventListener = null;
        }
    }
}