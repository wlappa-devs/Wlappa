using System;
using System.Threading.Tasks;
using Shared.Protos;

namespace Server.Application.ChainOfResponsibilityUtils
{
    public interface IChannelToClient<in T> where T : ServerMessage
    {
        Guid Id { get; }
        string? Name { get; set; }

        Task SendMessage(T message);

        IChannelToClient<TNew> AsNewHandler<TNew>() where TNew : ServerMessage;
    }
}