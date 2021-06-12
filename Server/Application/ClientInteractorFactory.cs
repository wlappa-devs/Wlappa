using System.Collections.Generic;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using Server.Application.ChainOfResponsibilityUtils;
using Shared.Protos;

namespace Server.Application
{
    public class ClientInteractorFactory
    {
        private readonly ILogger<ClientInteractor> _logger;
        private readonly ClientRouter _clientRouter;
        private readonly ChainResolver _chainResolver;

        public ClientInteractorFactory(ILogger<ClientInteractor> logger, ClientRouter clientRouter,
            ChainResolver chainResolver)
        {
            _logger = logger;
            _clientRouter = clientRouter;
            _chainResolver = chainResolver;
        }

        public ClientInteractor Create(IAsyncEnumerable<ClientMessage> request,
            ChannelWriter<ServerMessage> response) => new(request, response, _logger, _clientRouter, _chainResolver);
    }
}