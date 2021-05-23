using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Server.Games.Meta;
using Server.Routing.Helpers;
using Shared.Protos;

namespace Server.Routing
{
    public class MainController
    {
        private readonly ILogger<MainController> _logger;
        private readonly GameResolver _gameResolver;
        private readonly GameControllerFactory _gameControllerFactory;
        private static Dictionary<Guid, GameController> _games = new();

        public MainController(ILogger<MainController> logger, GameResolver gameResolver,
            GameControllerFactory gameControllerFactory)
        {
            _logger = logger;
            _gameResolver = gameResolver;
            _gameControllerFactory = gameControllerFactory;
        }

        public async Task ConnectClientToGame(Client player, Guid gameId)
        {
            if (!_games.ContainsKey(gameId))
            {
                await player.HandleLobbyMessage(new LobbyNotFound());
            }
            await _games[gameId].ConnectPlayer(player);
        }

        public Guid CreateGame(Guid hostId, GameTypes type, GameConfiguration config)
        {
            var gameFactory = _gameResolver.FindGameFactory(type);
            var newId = Guid.NewGuid();
            var gameController = _gameControllerFactory.Create(gameFactory, config, hostId, () => _games.Remove(newId));
            _games[newId] = gameController;
            return newId;
        }
    }
}