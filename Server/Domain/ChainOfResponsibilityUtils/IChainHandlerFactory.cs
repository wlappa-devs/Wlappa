using System;
using Shared.Protos;

namespace Server.Domain.ChainOfResponsibilityUtils
{
    public interface IChainHandlerFactory
    {
        IClientEventSubscriber<ClientMessage> Create(Guid id);
        void Remove(Guid id);
    }
}