using System;
using System.Collections.Generic;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Server.Domain.Lobby;
using Shared.Protos;

namespace Server.Application
{
    interface IDialog<out TClient, in TServer> where TClient : ClientMessage where TServer : ServerMessage
    {
        public Guid Id { get; }
        public string Name { get; }

        public void SendToClient(TServer message);

        public Func<IDialog<TClient, TServer>, TClient, Task> EventListener { set; }
    }

    public class ClientInteractor : ILobbyClientInteractor
    {
        private readonly IAsyncEnumerable<ClientMessage> _request;
        private readonly ChannelWriter<ServerMessage> _response;
        private readonly ILogger<ClientInteractor> _logger;
        private readonly ClientRouter _clientRouter;
        public Guid Id { get; } = Guid.NewGuid();
        public string? Name { get; private set; }

        public Func<ClientInteractor, LobbyClientMessage, Task>? LobbyEventListener { private get; set; }
        public Func<ClientInteractor, InGameClientMessage, Task>? InGameEventListener { private get; set; }

        public ClientInteractor(IAsyncEnumerable<ClientMessage> request,
            ChannelWriter<ServerMessage> response, ILogger<ClientInteractor> logger, ClientRouter clientRouter)
        {
            _request = request;
            _response = response;
            _logger = logger;
            _clientRouter = clientRouter;
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
                    var gameId = _clientRouter.CreateGame(Id, m.Configuration);
                    await _response.WriteAsync(new LobbyCreated()
                    {
                        Guid = gameId
                    });
                    return;
                case JoinLobby m:
                    await _clientRouter.ConnectClientToGame(this, m.Id);
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