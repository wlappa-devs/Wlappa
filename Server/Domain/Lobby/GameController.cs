using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Server.Domain.Games.Meta;
using Shared.Protos;

namespace Server.Domain.Lobby
{
    public class GameController
    {
        private readonly IGameFactory _factory;
        private readonly ConcurrentDictionary<Guid, string> _playersToRoles = new();
        private readonly List<ILobbyClientInteractor> _players = new(); // TODO make concurrent
        private readonly GameConfiguration _config;
        // ReSharper disable once NotAccessedField.Local
        private readonly ILogger _logger;
        private readonly Action _finished;
        private Guid Host { get; set; }
        private Game? _game;
        private bool _hasFinished;
        private GameTypes Type => _factory.Type;

        public GameController(IGameFactory factory, GameConfiguration config, Guid initialHost, ILogger logger,
            Action finished)
        {
            _factory = factory;
            _config = config;
            _logger = logger;
            _finished = finished;
            Host = initialHost;
        }

        public async Task ConnectPlayer(ILobbyClientInteractor player)
        {
            if (_game is not null)
            {
                await player.HandleLobbyMessage(new GameAlreadyStarted());
                return;
            }

            _players.Add(player);
            _playersToRoles[player.Id] = _factory.DefaultRole;
            player.LobbyEventListener = LobbyEventListener;
            player.InGameEventListener = HandleGameEvent;
            await player.HandleLobbyMessage(new JoinedLobby()
            {
                Type = Type,
                IsHost = player.Id == Host,
                AvailableRoles = _factory.Roles
            });
            await NotifyLobbyUpdate();
        }

        private async Task CreateGame(ILobbyClientInteractor initiator)
        {
            var payload = new GameCreationPayload(_playersToRoles, _players.ToArray());
            var message = _factory.ValidateConfig(_config, payload);
            if (message is null)
            {
                _game = _factory.Create(_config, payload, HandleGameFinish);
                var notification = new GameCreated();
                await Task.WhenAll(_players.Select(p => p.HandleLobbyMessage(notification)));
                await _game.Initialize();
                return;
            }

            await initiator.HandleLobbyMessage(new ConfigurationInvalid()
            {
                Message = message
            });
        }

        private async Task HandleGameEvent(IInGameClientInteractor clientInteractor, InGameClientMessage message)
        {
            if (_game is null) return;
            await _game.HandleEvent(clientInteractor, message);
        }

        private async Task HandleGameFinish()
        {
            _game = null;
            var notification = new GameFinished();
            await Task.WhenAll(_players.Select(p => p.HandleLobbyMessage(notification)));
        }

        private async Task LobbyEventListener(ILobbyClientInteractor clientInteractor, LobbyClientMessage message)
        {
            switch (message)
            {
                case ChangeRole m:
                {
                    if (clientInteractor.Id != Host) return;
                    if (_factory.Roles.Contains(m.NewRole))
                        _playersToRoles[m.PlayerId] = m.NewRole;

                    await NotifyLobbyUpdate();
                    return;
                }
                case StartGame:
                    if (clientInteractor.Id != Host) return;
                    await CreateGame(clientInteractor);
                    return;
                case Disconnect:
                    _playersToRoles.Remove(clientInteractor.Id, out _);
                    _players.RemoveAll(c => c.Id == clientInteractor.Id);
                    await NotifyLobbyUpdate();
                    if (!_players.Any()) HandleFinishDisconnection();

                    if (_players.All(x => x.Id != Host))
                    {
                        var msg = new LobbyDestroyed {Msg = "Host left"};
                        await Task.WhenAll(_players.Select(player => player.HandleLobbyMessage(msg)));
                        HandleFinishDisconnection();
                    }

                    return;
            }
        }

        private void HandleFinishDisconnection()
        {
            if (_hasFinished) return;
            _hasFinished = true;
            _finished();
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