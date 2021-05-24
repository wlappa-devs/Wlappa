using System;
using System.Threading.Tasks;
using Shared.Protos;

namespace Server.Routing.Helpers
{
    public interface ILobbyClient: IInGameClient
    {
        public Func<Client, LobbyClientMessage, Task> LobbyEventListener { set; }
        public Func<Client, InGameClientMessage, Task> InGameEventListener {  set; }
        public Task HandleLobbyMessage(LobbyServerMessage message);
    }
}