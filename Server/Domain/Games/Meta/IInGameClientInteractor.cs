using System;
using System.Threading.Tasks;
using Shared.Protos;

namespace Server.Domain.Games.Meta
{
    public interface IInGameClientInteractor
    {
        public Guid Id { get; }
        public string? Name { get; }
        public Task HandleInGameMessage(InGameServerMessage message);
    }
}