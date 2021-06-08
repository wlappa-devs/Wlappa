using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using ProtoBuf.Grpc;
using Shared.Protos;

namespace Client_lib
{
    public class Lobby
    {
        private readonly ChannelWriter<ClientMessage> _request;
        private readonly ChannelReader<ServerMessage> _response;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private Game? _gameInstance;
        public GameTypes Type { get; }
        public IReadOnlyList<string> AvailableRoles { get; }
        public bool AmHost { get; }
        
        public event Action? LobbyUpdate;
        public event Action? GameFinished;
        public event Action<Game>? HandleGameStart;

        public event Action<string>? LobbyDestroyed;
        public event Action<string>? ConfigurationInvalid; 

        public IReadOnlyCollection<PlayerInLobby>? LastLobbyStatus { get; private set; }
        public bool GameIsGoing { get; private set; }
        
        public Guid LobbyId { get; }
        public Guid ClientId { get; }

        public Lobby(GameTypes type, IReadOnlyList<string> availableRoles, bool amHost,
            ChannelWriter<ClientMessage> request, ChannelReader<ServerMessage> response, Guid lobbyId, Guid clientId)
        {
            _request = request;
            _response = response;
            LobbyId = lobbyId;
            ClientId = clientId;
            Type = type;
            AvailableRoles = availableRoles;
            AmHost = amHost;
        }

        public async Task StartProcessing()
        {
            await foreach (var message in _response.AsAsyncEnumerable()
                .WithCancellation(_cts.Token)
                .ConfigureAwait(false))
            {
                await Console.Out.WriteLineAsync(message.ToString());
                switch (message)
                {
                    case GameCreated _:
                        _gameInstance = new Game(_request, ClientId);
                        GameIsGoing = true;
                        HandleGameStart?.Invoke(_gameInstance);
                        continue;
                    case LobbyServerMessage m:
                        HandleLobbyEvent(m);
                        continue;
                    case InGameServerMessage m:
                        _gameInstance?.HandleGameEvent(m);
                        continue;
                }
            }
        }

        public async Task Disconnect()
        {
            await _request.WriteAsync(new Disconnect()); // TODO check disconnect implementation
            await Task.Delay(TimeSpan.FromSeconds(1));
            // _request.Complete();
            _cts.Cancel();
        }

        public async Task ChangeRole(Guid playerId, string newRole)
        {
            if (!AmHost) throw new InvalidOperationException();
            await _request.WriteAsync(new ChangeRole()
            {
                PlayerId = playerId,
                NewRole = newRole,
            });
        }

        public async Task StartGame()
        {
            if (!AmHost) throw new InvalidOperationException();
            await _request.WriteAsync(new StartGame());
        }

        private void HandleLobbyEvent(LobbyServerMessage message)
        {
            switch (message)
            {
                case LobbyDestroyed lobbyDestroyed:
                    LobbyDestroyed?.Invoke(lobbyDestroyed.Msg);
                    return;
                case LobbyUpdate update:
                    LastLobbyStatus = update.Players;
                    LobbyUpdate?.Invoke();
                    return;
                case ConfigurationInvalid msg :
                    ConfigurationInvalid?.Invoke(msg.Message);
                    return;
                case GameFinished _:
                    GameIsGoing = false;
                    _gameInstance = null;
                    GameFinished?.Invoke();
                    return;
            }
        }
    }
}