﻿using System;
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
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string? Name { get; private set; }
        public Guid Id { get; private set; }


        private ChannelReader<ServerMessage>? _responseReader;
        private ChannelWriter<ClientMessage>? _requestWriter;
        private Grpc.Core.Channel? _channel;

        public async Task ConnectToServer(string address, int port, string name)
        {
            GrpcClientFactory.AllowUnencryptedHttp2 = true;
            _channel = new Grpc.Core.Channel(address, port, ChannelCredentials.Insecure);
            await _channel.ConnectAsync(DateTime.Now.ToUniversalTime() + TimeSpan.FromSeconds(1));
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

            return lobbyServerMessage switch
            {
                GameAlreadyStarted _ => throw new GameAlreadyStartedException(),
                LobbyNotFound _ => throw new LobbyNotFoundException(),
                JoinedLobby lobbyInfo => new Lobby(lobbyInfo.Type, lobbyInfo.AvailableRoles, lobbyInfo.IsHost,
                    _requestWriter, _responseReader, lobbyId, Id),
                _ => throw new InvalidOperationException()
            };
        }

        public async Task ChangeName(string newName)
        {
            if (_requestWriter != null)
                await _requestWriter.WriteAsync(new ChangeName()
                {
                    NewName = newName,
                });
        }
    }
}