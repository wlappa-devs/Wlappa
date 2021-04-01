using System.Collections.Generic;
using Server.Routing;

namespace Server.Games.Meta
{
    public class GameController
    {
        private readonly IGameFactory _factory;
        private readonly Dictionary<IPlayer, string> _playersToRoles = new();
        private readonly IGameConfiguration _config;
        public long Host { get; set; }
        private IGame? _game;
        public GameType Type { get; }

        public GameController(IGameFactory factory, IGameConfiguration config, long initialHost, GameType type)
        {
            _factory = factory;
            _config = config;
            Type = type;
            Host = initialHost;
        }

        public void ConnectPlayer(IPlayer player, IPlayerConnection connection)
        {
            if (_game is not null)
            {
                player.HandleEvent(new GameIsGoing());
                return;
            }

            _playersToRoles.Add(player, _factory.Roles[0]);
            connection.AddEventListener(HandleEvent);
            var playerConnectionEvent = new PlayerConnected
            {
                Id = player.Id,
                Name = player.Name,
            };
            foreach (var playerToSend in _playersToRoles.Keys)
            {
                playerToSend.HandleEvent(playerConnectionEvent);
            }
        }

        private void CreateGame()
        {
        }

        private void HandleEvent(IPlayerEvent e)
        {
        }
    }

    public class PlayerConnected : IToPlayerEvent
    {
        public long? Id { get; init; }
        public string? Name { get; init; }
    }

    public class GameIsGoing : IToPlayerEvent
    {
    }
}