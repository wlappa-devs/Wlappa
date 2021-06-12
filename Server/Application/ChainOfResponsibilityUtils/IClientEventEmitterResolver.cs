using System;
using Shared.Protos;

namespace Server.Application.ChainOfResponsibilityUtils
{
    public interface IClientEventEmitterResolver<out T> where T : ClientMessage
    {
        IClientEventEmitter<T>? Resolve(Guid id);
    }
}