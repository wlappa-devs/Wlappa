using System;
using Shared.Protos;

namespace Server.Domain.ChainOfResponsibilityUtils
{
    public interface IClientEventEmitterResolver<out T> where T : ClientMessage
    {
        IClientEventEmitter<T>? Resolve(Guid id);
    }
}