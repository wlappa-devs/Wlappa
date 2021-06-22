using System.Collections.Generic;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using Server.Domain.ChainOfResponsibilityUtils;
using Shared.Protos;

namespace Server.Application
{
    public class ClientInteractorFactory
    {
        private readonly ILogger<ClientInteractor> _logger;
        private readonly ChainResolver _chainResolver;

        public ClientInteractorFactory(ILogger<ClientInteractor> logger, ChainResolver chainResolver)
        {
            _logger = logger;
            _chainResolver = chainResolver;
        }

        public ClientInteractor Create(IAsyncEnumerable<ClientMessage> request,
            ChannelWriter<ServerMessage> response) => new(request, response, _logger, _chainResolver);
    }
}