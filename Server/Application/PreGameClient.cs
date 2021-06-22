using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Server.Application.ChainOfResponsibilityUtils;
using Shared.Protos;

namespace Server.Application
{
    public class PreGameClient : IClientEventSubscriber<PreGameClientMessage>
    {
        private readonly ILogger<PreGameClient> _logger;
        private readonly IChannelToClient<PreGameServerMessage> _channelToClient;
        private readonly ClientRouter _clientRouter;

        public PreGameClient(ILogger<PreGameClient> logger, IChannelToClient<PreGameServerMessage> channelToClient,
            ClientRouter clientRouter)
        {
            _logger = logger;
            _channelToClient = channelToClient;
            _clientRouter = clientRouter;
        }

        public async Task EventHandle(Guid? clientId, PreGameClientMessage clientMessage)
        {
            _logger.LogInformation(clientMessage.ToString());
            switch (clientMessage)
            {
                case Greeting m:
                    _channelToClient.Name = m.Name;
                    await _channelToClient.SendMessage(new GreetingSuccessful()
                    {
                        Guid = _channelToClient.Id
                    });
                    return;
                case CreateLobby m:
                    var gameId = _clientRouter.CreateLobby(_channelToClient.Id, m.Configuration);
                    await _channelToClient.SendMessage(new LobbyCreated()
                    {
                        Guid = gameId
                    });
                    return;
                case JoinLobby m:
                    await _clientRouter.ConnectClientToLobby(_channelToClient.AsNewHandler<ServerMessage>(), m.Id);
                    return;
                case ChangeName m:
                    _channelToClient.Name = m.NewName;
                    return;
            }
        }
    }
}