using System;
using System.Threading.Tasks;
using Server.Application;
using Server.Domain.Games.Meta;
using Shared.Protos;

namespace Server.Domain.Lobby
{
    public interface ILobbyClientInteractor: IInGameClientInteractor
    {
        public Func<ClientInteractor, LobbyClientMessage, Task> LobbyEventListener { set; }
        public Func<ClientInteractor, InGameClientMessage, Task> InGameEventListener {  set; }
        public Task HandleLobbyMessage(LobbyServerMessage message);
    }
}