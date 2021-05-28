using System;
using System.IO;
using System.Threading.Tasks;
using ProtoBuf.Grpc.Client;
using Shared.Protos;
using System.Threading.Channels;
using Grpc.Core;
using ProtoBuf.Grpc;
using Channel = System.Threading.Channels.Channel;

namespace Client_lib
{
    public class Client
    {
        public string? Name { get; private set; }
        public Guid Id { get; private set; }
        
        
        private ChannelReader<ServerMessage>? _responseReader;
        private ChannelWriter<ClientMessage>? _requestWriter;
        private Grpc.Core.Channel? _channel;

        public async Task ConnectToServer(string address, string name)
        {
            GrpcClientFactory.AllowUnencryptedHttp2 = true;
            _channel = new Grpc.Core.Channel(address, ChannelCredentials.Insecure);
            var grpcClient = _channel.CreateGrpcService<IMainServiceContract>();
            var request = Channel.CreateUnbounded<ClientMessage>();
            var responseStream = grpcClient.Connect(request.AsAsyncEnumerable());
            _responseReader = responseStream.AsChannelReader();
            _requestWriter = request.Writer;
            await ReceiveId(name);
        }

        private async Task ReceiveId(string name)
        {
            Name = name;
            await _requestWriter!.WriteAsync(new Greeting()
            {
                Name = name,
            });

            var response = await _responseReader!.ReadAsync();
            if (!(response is GreetingSuccessful greetingSuccessful))
                throw new InvalidOperationException();
            Id = greetingSuccessful.Guid;
        }

        public async Task<Guid> CreateGame(GameConfiguration configuration)
        {
            if (_requestWriter is null || _responseReader is null)
                throw new InvalidOperationException();
            await _requestWriter.WriteAsync(new CreateLobby()
            {
                Configuration = configuration,
            });

            var response = await _responseReader.ReadAsync();

            if (!(response is LobbyCreated lobbyCreated))
                throw new InvalidOperationException();

            return lobbyCreated.Guid;
        }

        public async Task<Lobby> JoinGame(Guid lobbyId)
        {
            if (_requestWriter is null || _responseReader is null)
                throw new InvalidOperationException();
            await _requestWriter.WriteAsync(new JoinLobby()
            {
                Id = lobbyId,
            });

            var response = await _responseReader.ReadAsync();

            if (!(response is LobbyServerMessage lobbyServerMessage))
                throw new InvalidOperationException();

            switch (lobbyServerMessage)
            {
                case GameAlreadyStarted _:
                    throw new GameAlreadyStartedException();
                case LobbyNotFound _:
                    throw new LobbyNotFoundException();
                case JoinedLobby lobbyInfo:
                    return new Lobby(lobbyInfo.Type, lobbyInfo.AvailableRoles, lobbyInfo.IsHost, _requestWriter,
                        _responseReader, lobbyId, Id);
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}