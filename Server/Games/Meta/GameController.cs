using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Server.Routing;
using Server.Routing.Helpers;
using Shared.Protos;

namespace Server.Games.Meta
{
    public class GameController
    {
        private readonly IGameFactory _factory;
        private readonly Dictionary<Guid, string> _playersToRoles = new();
        private readonly List<Client> _players = new();
        private readonly GameConfiguration _config;
        private readonly ILogger _logger;
        private readonly Action _finished;
        public Guid Host { get; set; }
        private Game? _game;
        public GameTypes Type => _factory.Type;

        public GameController(IGameFactory factory, GameConfiguration config, Guid initialHost, ILogger logger,
            Action finished)
        {
            _factory = factory;
            _config = config;
            _logger = logger;
            _finished = finished;
            Host = initialHost;
        }

        public async Task ConnectPlayer(Client player)
        {
            if (_game is not null)
            {
                await player.HandleLobbyMessage(new GameAlreadyStarted());
                return;
            }

            _players.Add(player);
            _playersToRoles[player.Id] = _factory.DefaultRole;
            player.LobbyEventListener = LobbyEventListener;
            await player.HandleLobbyMessage(new JoinedLobby()
            {
                Type = Type,
                IsHost = player.Id == Host,
                AvailableRoles = _factory.Roles
            });
            await NotifyLobbyUpdate();
        }

        private async Task CreateGame(Client initiator)
        {
            var payload = new GameCreationPayload(_playersToRoles);
            var message = _factory.ValidateConfig(_config, payload);
            if (message is null)
            {
                _game = _factory.Create(_config, payload, _players, HandleGameFinish);
                var notification = new GameCreated();
                await Task.WhenAll(_players.Select(p => p.HandleLobbyMessage(notification)));
                foreach (var player in _players)
                {
                    player.InGameEventListener = HandleGameEvent;
                }

                return;
            }

            await initiator.HandleLobbyMessage(new ConfigurationInvalid()
            {
                Message = message
            });
        }

        private async Task HandleGameEvent(IInGameClient client, InGameClientMessage message)
        {
            if (_game is null) return;
            await _game.semaphore.WaitAsync();
            try
            {
                await _game.HandleEvent(client, message);
            }
            finally
            {
                _game.semaphore.Release();
            }
        }

        // TODO disable players event listeners
        private async Task HandleGameFinish()
        {
            _game = null;
            var notification = new GameFinished();
            await Task.WhenAll(_players.Select(p => p.HandleLobbyMessage(notification)));
        }

        private async Task LobbyEventListener(Client client, LobbyClientMessage message)
        {
            if (client.Id != Host) return; //TODO
            switch (message)
            {
                case ChangeRole m:
                {
                    if (_factory.Roles.Contains(m.NewRole))
                        _playersToRoles[m.PlayerId] = m.NewRole;

                    await NotifyLobbyUpdate();
                    return;
                }
                case StartGame:
                    await CreateGame(client);
                    return;
                case Disconnect:
                    _playersToRoles.Remove(client.Id);
                    _players.RemoveAll(c => c.Id == client.Id);
                    return;
            }
        }

        private async Task NotifyLobbyUpdate()
        {
            var message = new LobbyUpdate()
            {
                Players = _players.Select(p =>
                {
                    var role = _playersToRoles.ContainsKey(p.Id) ? _playersToRoles[p.Id] : "";
                    return new PlayerInLobby()
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Role = role,
                    };
                }).ToArray()
            };
            await Task.WhenAll(_players.Select(p => p.HandleLobbyMessage(message)));
        }
    }
}