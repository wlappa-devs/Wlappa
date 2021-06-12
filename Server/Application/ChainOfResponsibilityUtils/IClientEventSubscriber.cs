using System;
using System.Threading.Tasks;
using Shared.Protos;

namespace Server.Application.ChainOfResponsibilityUtils
{
    public interface IClientEventSubscriber<in T> where T : ClientMessage
    {
        Task EventHandle(Guid? clientId, T e);
    }
}