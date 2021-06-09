using System;
using System.Collections.Generic;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Server.Domain.Lobby;
using Shared.Protos;

namespace Server.Application
{
    public class ClientInteractor : ILobbyClientInteractor
    {
        private readonly IAsyncEnumerable<ClientMessage> _request;
        private readonly ChannelWriter<ServerMessage> _response;
        private readonly ILogger<ClientInteractor> _logger;
        private readonly MainController _mainController;
        public Guid Id { get; } = Guid.NewGuid();
        public string? Name { get; private set; }

        public Func<ClientInteractor, LobbyClientMessage, Task>? LobbyEventListener { private get; set; }
        public Func<ClientInteractor, InGameClientMessage, Task>? InGameEventListener { private get; set; }

        public ClientInteractor(IAsyncEnumerable<ClientMessage> request,
            ChannelWriter<ServerMessage> response, ILogger<ClientInteractor> logger, MainController mainController)
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
                        _logger.LogInformation($"Got event pregame {e}");
                        await HandlePregameMessage(e);
                        break;
                    case LobbyClientMessage e:
                        _logger.LogInformation($"Got event lobby {e}");
                        if (LobbyEventListener is not null)
                            await LobbyEventListener(this, e);
                        break;
                    case InGameClientMessage e:
                        _logger.LogInformation($"Got event ingame {e}");
                        if (InGameEventListener is not null)
                            await InGameEventListener(this, e);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private async Task HandlePregameMessage(PreGameClientMessage message)
        {
            _logger.LogInformation(message.ToString());
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
                    var gameId = _mainController.CreateGame(Id, m.Configuration);
                    await _response.WriteAsync(new LobbyCreated()
                    {
                        Guid = gameId
                    });
                    return;
                case JoinLobby m:
                    await _mainController.ConnectClientToGame(this, m.Id);
                    return;
                case ChangeName m:
                    Name = m.NewName;
                    return;
            }
        }

        public async Task HandleInGameMessage(InGameServerMessage message) => await _response.WriteAsync(message);

        public async Task HandleLobbyMessage(LobbyServerMessage message) => await _response.WriteAsync(message);

        public async void HandleConnectionFailure()
        {
            _logger.LogInformation("Got unstable disconnect");
            if (LobbyEventListener != null) await LobbyEventListener(this, new Disconnect());
        }
    }
}