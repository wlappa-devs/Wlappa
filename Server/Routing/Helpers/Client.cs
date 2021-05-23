using System;
using System.Collections.Generic;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Server.Routing.Helpers
{
    public class ClientFactory
    {
        private readonly ILogger<Client> _logger;
        private readonly MainController _mainController;

        public ClientFactory(ILogger<Client> logger, MainController mainController)
        {
            _logger = logger;
            _mainController = mainController;
        }

        public Client Create(IAsyncEnumerable<ClientMessage> request,
            ChannelWriter<ServerMessage> response) => new(request, response, _logger, _mainController);
    }

    public class Client
    {
        private readonly IAsyncEnumerable<ClientMessage> _request;
        private readonly ChannelWriter<ServerMessage> _response;
        private readonly ILogger<Client> _logger;
        private readonly MainController _mainController;
        public Guid Id { get; } = Guid.NewGuid();
        public string? Name { get; private set; }

        public Func<Client, LobbyClientMessage, Task> LobbyEventListener { private get; set; }
        public Func<Client, InGameClientMessage, Task> InGameEventListener { private get; set; }

        public Client(IAsyncEnumerable<ClientMessage> request,
            ChannelWriter<ServerMessage> response, ILogger<Client> logger, MainController mainController)
        {
            _request = request;
            _response = response;
            _logger = logger;
            _mainController = mainController;
        }

        public async Task StartProcessing()
        {
            await foreach (var message in _request)
            {
                switch (message)
                {
                    case PreGameClientMessage e:
                        _logger.LogInformation("Got event pregame");
                        await HandlePregameMessage(e);
                        break;
                    case LobbyClientMessage e:
                        _logger.LogInformation("Got event lobby");
                        await LobbyEventListener(this, e);
                        break;
                    case InGameClientMessage e:
                        _logger.LogInformation("Got event ingame");
                        await InGameEventListener(this, e);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private async Task HandlePregameMessage(PreGameClientMessage message)
        {
            switch (message)
            {
                case Greeting m:
                    Name = m.Name;
                    await _response.WriteAsync(new GreetingSuccessful()
                    {
                        Guid = Id
                    });
                    return;
                case CreateLobby m:
                    var gameId = _mainController.CreateGame(Id, m.Type, m.Configuration);
                    await _response.WriteAsync(new LobbyCreated()
                    {
                        Guid = gameId
                    });
                    return;
                case JoinGame m:
                    await _mainController.ConnectClientToGame(this, m.Id);
                    return;
            }
        }

        public async Task HandleInGameMessage(InGameServerMessage message)
        {
            await _response.WriteAsync(message);
        }

        public async Task HandleLobbyMessage(LobbyServerMessage message)
        {
            await _response.WriteAsync(message);
        }
    }
}