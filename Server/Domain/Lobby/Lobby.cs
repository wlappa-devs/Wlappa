using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Server.Domain.Games.Meta;
using Shared.Protos;

namespace Server.Domain.Lobby
{
    internal class SynchronisedPlayersState
    {
        private readonly Dictionary<Guid, string> _playersToRoles = new();
        private readonly List<ILobbyClientInteractor> _players = new();
        private readonly ReaderWriterLockSlim _lock = new();

        public void AddPlayerWithRole(ILobbyClientInteractor player, string role)
        {
            _lock.EnterWriteLock();
            try
            {
                _players.Add(player);
                _playersToRoles[player.Id] = role;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void ChangePlayerRole(Guid id, string newRole)
        {
            _lock.EnterWriteLock();
            try
            {
                _playersToRoles[id] = newRole;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public IReadOnlyList<ILobbyClientInteractor> RemovePlayer(Guid id)
        {
            _lock.EnterWriteLock();
            try
            {
                _players.RemoveAll(p => p.Id == id);
                _playersToRoles.Remove(id);
                return _players.ToImmutableArray();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public T ReadWithLock<T>(
            Func<IReadOnlyList<ILobbyClientInteractor>, IReadOnlyDictionary<Guid, string>, T> action)
        {
            _lock.EnterReadLock();
            try
            {
                return action(_players.ToImmutableArray(), _playersToRoles.ToImmutableDictionary());
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public void ReadWithLock(
            Action<IReadOnlyList<ILobbyClientInteractor>, IReadOnlyDictionary<Guid, string>> action)
        {
            _lock.EnterReadLock();
            try
            {
                action(_players, _playersToRoles);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        ~SynchronisedPlayersState()
        {
            _lock.Dispose();
        }
    }

    public class Lobby
    {
        private readonly IGameFactory _factory;
        private readonly SynchronisedPlayersState _playersState = new();
        private readonly GameConfiguration _config;

        // ReSharper disable once NotAccessedField.Local
        private readonly ILogger _logger;
        private readonly Action _finished;
        private readonly Guid _host;
        private Game? _game;
        private bool _hasFinished;
        private GameTypes Type => _factory.Type;

        public Lobby(IGameFactory factory, GameConfiguration config, Guid initialHost, ILogger logger,
            Action finished)
        {
            _factory = factory;
            _config = config;
            _logger = logger;
            _finished = finished;
            _host = initialHost;
        }

        public async Task ConnectPlayer(ILobbyClientInteractor player)
        {
            if (_game is not null)
            {
                await player.HandleLobbyMessage(new GameAlreadyStarted());
                return;
            }

            _playersState.AddPlayerWithRole(player, _factory.DefaultRole);
            player.LobbyEventListener = LobbyEventListener;
            player.InGameEventListener = HandleGameEvent;
            await player.HandleLobbyMessage(new JoinedLobby()
            {
                Type = Type,
                IsHost = player.Id == _host,
                AvailableRoles = _factory.Roles
            });
            await NotifyLobbyUpdate();
        }

        private async Task CreateGame(ILobbyClientInteractor initiator)
        {
            var (payload, message, players) = _playersState.ReadWithLock((playersInState, playersToRoles) =>
            {
                var payloadInState = new GameCreationPayload(playersToRoles, playersInState.ToArray());
                var messageInState = _factory.ValidateConfig(_config, payloadInState);
                return (payloadInState, messageInState, playersInState);
            });

            if (message is null)
            {
                _game = _factory.Create(_config, payload, HandleGameFinish);
                var notification = new GameCreated();
                await Task.WhenAll(players.Select(p => p.HandleLobbyMessage(notification)));
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
            var players = _playersState.ReadWithLock((playersInState, _) =>
            {
                _game = null;
                return playersInState;
            });

            var notification = new GameFinished();
            await Task.WhenAll(players.Select(p => p.HandleLobbyMessage(notification)));
        }

        private async Task LobbyEventListener(ILobbyClientInteractor clientInteractor, LobbyClientMessage message)
        {
            switch (message)
            {
                case ChangeRole m:
                {
                    if (clientInteractor.Id != _host) return;
                    if (_factory.Roles.Contains(m.NewRole))
                        _playersState.ChangePlayerRole(m.PlayerId, m.NewRole);

                    await NotifyLobbyUpdate();
                    return;
                }
                case StartGame:
                    if (clientInteractor.Id != _host) return;
                    await CreateGame(clientInteractor);
                    return;
                case Disconnect:
                    var players = _playersState.RemovePlayer(clientInteractor.Id);
                    await NotifyLobbyUpdate();
                    if (!players.Any()) HandleFinishDisconnection();

                    if (players.All(x => x.Id != _host))
                    {
                        var msg = new LobbyDestroyed {Msg = "Host left"};
                        await Task.WhenAll(players.Select(player => player.HandleLobbyMessage(msg)));
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
            var (message, players) = _playersState.ReadWithLock((playersInState, playersToRoles) => (new LobbyUpdate()
            {
                Players = playersInState.Select(p =>
                {
                    var role = playersToRoles.ContainsKey(p.Id) ? playersToRoles[p.Id] : "";
                    return new PlayerInLobby()
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Role = role,
                    };
                }).ToArray()
            }, playersInState));
            await Task.WhenAll(players.Select(p => p.HandleLobbyMessage(message)));
        }
    }
}