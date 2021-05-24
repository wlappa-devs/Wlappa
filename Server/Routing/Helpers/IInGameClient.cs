using System;
using System.Threading.Tasks;
using Shared.Protos;

namespace Server.Routing.Helpers
{
    public interface IInGameClient
    {
        public Guid Id { get; }
        public string? Name { get; }
        public Task HandleInGameMessage(InGameServerMessage message);
    }
}