using System;
using System.Threading.Tasks;
using Shared.Protos;

namespace Server.Application.ChainOfResponsibilityUtils
{
    public class EventFilter<T> : IClientEventSubscriber<ClientMessage>, IClientEventEmitter<T>
        where T : ClientMessage
    {
        public Func<Guid?, T, Task>? EventListener { private get; set; }

        public async Task EventHandle(Guid? clientId, ClientMessage clientMessage)
        {
            if (clientMessage is T correctEvent && EventListener is not null)
                await EventListener(clientId, correctEvent);
        }
    }
}