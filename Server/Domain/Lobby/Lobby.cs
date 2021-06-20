using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Server.Application.ChainOfResponsibilityUtils;
using Server.Domain.Games.Meta;
using Shared.Protos;

namespace Server.Domain.Lobby
{
    public class Lobby : IClientEventSubscriber<LobbyClientMessage>
    {
        private readonly IGameFactory _factory;
        private readonly SynchronisedPlayersState _playersState = new();
        private readonly GameConfiguration _config;

        // ReSharper disable once NotAccessedField.Local
        private readonly ILogger _logger;
        private readonly Action<IReadOnlyCollection<Guid>>? _finished;
        private readonly SubscriptionManager<InGameClientMessage> _subscriptionManager;
        private readonly Guid _hostId;
        private Game? _game;
        private bool _hasFinished;
        private GameTypes Type => _factory.Type;
        private IChannelToClient<LobbyServerMessage>? _host;

        public Lobby(IGameFactory factory, GameConfiguration config, Guid initialHostId, ILogger logger,
            Action<IReadOnlyCollection<Guid>>? finished, SubscriptionManager<InGameClientMessage> subscriptionManager)
        {
            _factory = factory;
            _config = config;
            _logger = logger;
            _finished = finished;
            _subscriptionManager = subscriptionManager;
            _hostId = initialHostId;
        }

        public async Task ConnectPlayer(IChannelToClient<LobbyServerMessage> player)
        {
            if (_game is not null)
            {
                await player.SendMessage(new GameAlreadyStarted());
                return;
            }

            if (player.Id == _hostId) _host = player;
            _playersState.AddPlayerWithRole(player, _factory.DefaultRole);

            await player.SendMessage(new JoinedLobby()
            {
                Type = Type,
                IsHost = player.Id == _hostId,
                AvailableRoles = _factory.Roles
            });
            await NotifyLobbyUpdate();
        }

        private async Task CreateGame()
        {
            var (players, playersToRoles, playersToReadyState) =
                _playersState.ReadWithLock((p, p2R, p2Rd) => (p, p2R, p2Rd));
            if (playersToReadyState.Any(kw => kw.Value == false))
            {
                if (_host is not null)
                    await _host.SendMessage(new GameStartingProblems{Message = "Players are not ready"});
                return;
            }
            var payload = new GameCreationPayload(playersToRoles,
                players.Select(e => e.AsNewHandler<InGameServerMessage>()).ToArray());
            var message = _factory.ValidateConfig(_config, payload);
            if (message is null)
            {
                _game = _factory.Create(_config, payload, HandleGameFinish);
                var notification = new GameCreated();
                foreach (var player in players) _subscriptionManager.SubscribeForClient(player.Id, _game);

                await Task.WhenAll(players.Select(p => p.SendMessage(notification)));
                await _game.Initialize();
                _playersState.UnreadyEveryone();
                return;
            }

            if (_host is not null)
                await _host.SendMessage(new GameStartingProblems()
                {
                    Message = message
                });
        }

        private async Task HandleGameFinish()
        {
            var players = _playersState.ReadWithLock((playersInState, _, _) =>
            {
                _game = null;
                return playersInState;
            });
            foreach (var player in players) _subscriptionManager.UnsubscribeFromClient(player.Id);

            var notification = new GameFinished();
            await Task.WhenAll(players.Select(p => p.SendMessage(notification)));
        }

        public async Task EventHandle(Guid? cId, LobbyClientMessage message)
        {
            if (cId is null) return;
            var clientId = cId.Value;
            switch (message)
            {
                case ChangeRole m:
                {
                    if (clientId != _hostId) return;
                    if (_factory.Roles.Contains(m.NewRole))
                        _playersState.ChangePlayerRole(m.PlayerId, m.NewRole);

                    await NotifyLobbyUpdate();
                    return;
                }
                case StartGame:
                    if (clientId != _hostId) return;
                    await CreateGame();
                    await NotifyLobbyUpdate();
                    return;
                case Disconnect:
                    var players = _playersState.RemovePlayer(clientId);
                    var playerIds = players.Select(p => p.Id).ToImmutableArray();
                    await NotifyLobbyUpdate();
                    if (!players.Any()) HandleFinishDisconnection(playerIds);

                    if (players.All(x => x.Id != _hostId))
                    {
                        var msg = new LobbyDestroyed {Msg = "Host left"};
                        await Task.WhenAll(players.Select(player => player.SendMessage(msg)));
                        HandleFinishDisconnection(playerIds);
                    }

                    return;
                case ReadyChecked readyChecked:
                    _playersState.ChangePlayerReadyStatus(clientId, readyChecked.OneWayReady);
                    await NotifyLobbyUpdate();
                    return;
            }
        }

        private void HandleFinishDisconnection(IReadOnlyCollection<Guid> playerIds)
        {
            if (_hasFinished) return;
            _hasFinished = true;
            _finished?.Invoke(playerIds);
        }

        private async Task NotifyLobbyUpdate()
        {
            var (message, players) = 
                _playersState.ReadWithLock((playersInState, playersToRoles, playersToReadyStatus) => (new LobbyUpdate()
            {
                Players = playersInState.Select(p =>
                {
                    var role = playersToRoles.ContainsKey(p.Id) ? playersToRoles[p.Id] : "";
                    var readiness = playersToReadyStatus.GetValueOrDefault(p.Id);
                    return new PlayerInLobby()
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Role = role,
                        IsReady = readiness
                    };
                }).ToArray()
            }, playersInState));
            await Task.WhenAll(players.Select(p => p.SendMessage(message)));
        }
    }
}