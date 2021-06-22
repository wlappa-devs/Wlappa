using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Shared.Protos;

namespace Server.Domain.ChainOfResponsibilityUtils
{
    public class ChainHandlerManager<T> : IChainHandlerFactory, IClientEventEmitterResolver<T> where T : ClientMessage
    {
        private readonly ConcurrentDictionary<Guid, EventFilter<T>> _clients = new();

        public IClientEventSubscriber<ClientMessage> Create(Guid id) =>
            _clients.GetOrAdd(id, _ => new EventFilter<T>());

        public IClientEventEmitter<T>? Resolve(Guid id) => _clients.GetValueOrDefault(id);

        public void Remove(Guid id) => _clients.Remove(id, out _);
    }
}