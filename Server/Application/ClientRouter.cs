using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Server.Application.ChainOfResponsibilityUtils;
using Server.Domain.Lobby;
using Shared.Protos;

namespace Server.Application
{
    public class ClientRouter
    {
        // ReSharper disable once NotAccessedField.Local
        private readonly ILogger<ClientRouter> _logger;
        private readonly GameFactoryResolver _gameFactoryResolver;
        private readonly LobbyFactory _lobbyFactory;
        private readonly SubscriptionManager<LobbyClientMessage> _subscriptionManager;
        private readonly ConcurrentDictionary<Guid, Lobby> _lobbies = new();

        public ClientRouter(ILogger<ClientRouter> logger, GameFactoryResolver gameFactoryResolver,
            LobbyFactory lobbyFactory, SubscriptionManager<LobbyClientMessage> subscriptionManager)
        {
            _logger = logger;
            _gameFactoryResolver = gameFactoryResolver;
            _lobbyFactory = lobbyFactory;
            _subscriptionManager = subscriptionManager;
        }

        public async Task ConnectClientToLobby(IChannelToClient<LobbyServerMessage> player, Guid lobbyId)
        {
            if (!_lobbies.TryGetValue(lobbyId, out var lobby))
            {
                await player.SendMessage(new LobbyNotFound());
                return;
            }

            await lobby.ConnectPlayer(player);
            _subscriptionManager.SubscribeForClient(player.Id, lobby);
        }

        public Guid CreateLobby(Guid hostId, GameConfiguration config)
        {
            var gameFactory = _gameFactoryResolver.FindGameFactory(config.Type);
            var newId = Guid.NewGuid();
            var lobby =
                _lobbyFactory.Create(gameFactory, config, hostId, playerIds =>
                {
                    _lobbies.Remove(newId, out _);
                    foreach (var playerId in playerIds) _subscriptionManager.UnsubscribeFromClient(playerId);
                });
            _lobbies[newId] = lobby;
            return newId;
        }
    }
}