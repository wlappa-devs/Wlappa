using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Server.Routing;
using Server.Routing.Helpers;
using Shared.Protos;

namespace Server.Games.Meta
{
    public interface IGame
    {
        Task HandleEvent(Client client, InGameClientMessage e);
    }

    public class GameCreationPayload
    {
        public GameCreationPayload(IReadOnlyDictionary<Guid, string> playerToRole)
        {
            PlayerToRole = playerToRole;
        }

        public IReadOnlyDictionary<Guid, string> PlayerToRole { get; }
    }

    public interface IGameFactory
    {
        IGame Create(GameConfiguration config, GameCreationPayload payload, IReadOnlyCollection<Client> clients,
            Func<Task> finished);

        IReadOnlyList<string> Roles { get; }
        GameTypes Type { get; }

        string? ValidateConfig(GameConfiguration config, GameCreationPayload payload);
    }

    public interface IGameEvent
    {
    }

    public interface IPlayerEvent : IGameEvent
    {
        IPlayer Player { get; }
    }
}