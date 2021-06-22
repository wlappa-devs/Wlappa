using System;
using System.Threading.Tasks;
using Shared.Protos;

namespace Server.Domain.ChainOfResponsibilityUtils
{
    public interface IClientEventSubscriber<in T> where T : ClientMessage
    {
        Task EventHandle(Guid? clientId, T clientMessage);
    }
}