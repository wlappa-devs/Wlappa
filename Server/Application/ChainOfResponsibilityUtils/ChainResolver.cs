using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Shared.Protos;

namespace Server.Application.ChainOfResponsibilityUtils
{
    public class ChainResolver
    {
        private readonly IReadOnlyCollection<IChainHandlerFactory> _factories;

        public ChainResolver(IEnumerable<IChainHandlerFactory> factories)
        {
            _factories = factories.ToArray();
        }

        public Func<ClientMessage, Task> GetChainForClient(Guid id)
        {
            var handlers = _factories.Select(f => f.Create(id)).ToImmutableArray();
            return message => Task.WhenAll(handlers.Select(h => h.EventHandle(id, message)));
        }

        public void RemoveAllHandlers(Guid id)
        {
            foreach (var factory in _factories) factory.Remove(id);
        }
    }
}