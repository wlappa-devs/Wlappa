using Microsoft.Extensions.Logging;
using Server.Domain.ChainOfResponsibilityUtils;
using Shared.Protos;

namespace Server.Application
{
    public class PreGameClientFactory
    {
        private readonly ILogger<PreGameClient> _logger;
        private readonly ClientRouter _clientRouter;

        public PreGameClientFactory(ILogger<PreGameClient> logger,
            ClientRouter clientRouter)
        {
            _logger = logger;
            _clientRouter = clientRouter;
        }

        public PreGameClient Create(IChannelToClient<PreGameServerMessage> channelToClient) =>
            new(_logger, channelToClient, _clientRouter);
    }
}