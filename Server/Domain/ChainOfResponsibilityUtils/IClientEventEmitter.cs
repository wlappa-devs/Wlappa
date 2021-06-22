using System;
using System.Threading.Tasks;
using Shared.Protos;

namespace Server.Domain.ChainOfResponsibilityUtils
{
    public interface IClientEventEmitter<out T> where T : ClientMessage
    {
        Func<Guid?, T, Task>? EventListener { set; }
    }
}