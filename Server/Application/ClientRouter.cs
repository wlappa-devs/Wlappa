using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Server.Domain.Lobby;
using Shared.Protos;

namespace Server.Application
{
    public class ClientRouter
    {
        private readonly ILogger<ClientRouter> _logger;
        private readonly GameResolver _gameResolver;
        private readonly LobbyFactory _lobbyFactory;
        private readonly ConcurrentDictionary<Guid, Lobby> _games = new();

        public ClientRouter(ILogger<ClientRouter> logger, GameResolver gameResolver,
            LobbyFactory lobbyFactory)
        {
            _logger = logger;
            _gameResolver = gameResolver;
            _lobbyFactory = lobbyFactory;
        }

        public async Task ConnectClientToGame(ClientInteractor player, Guid gameId)
        {
            if (!_games.ContainsKey(gameId))
            {
                await player.HandleLobbyMessage(new LobbyNotFound());
            }

            await _games[gameId].ConnectPlayer(player);
        }

        public Guid CreateGame(Guid hostId, GameConfiguration config)
        {
            var gameFactory = _gameResolver.FindGameFactory(config.Type);
            var newId = Guid.NewGuid();
            var gameController =
                _lobbyFactory.Create(gameFactory, config, hostId, () => _games.Remove(newId, out _));
            _games[newId] = gameController;
            return newId;
        }
    }
}