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

        public IReadOnlyCollection<PlayerInLobby>? LastLobbyStatus { get; private set; }
        public bool GameIsGoing { get; private set; }
        
        public Guid Id { get; }

        public Lobby(GameTypes type, IReadOnlyList<string> availableRoles, bool amHost,
            ChannelWriter<ClientMessage> request, ChannelReader<ServerMessage> response, Guid id)
        {
            _request = request;
            _response = response;
            Id = id;
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
                        _gameInstance = new Game(_request);
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
            _request.Complete();
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
                case LobbyUpdate update:
                    LastLobbyStatus = update.Players;
                    LobbyUpdate?.Invoke();
                    return;
                case GameFinished _:
                    GameIsGoing = false;
                    GameFinished?.Invoke();
                    return;
            }
        }
    }
}