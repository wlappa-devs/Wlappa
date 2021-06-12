using System.Collections.Generic;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using Shared.Protos;

namespace Server.Application
{
    public class ClientInteractorFactory
    {
        private readonly ILogger<ClientInteractor> _logger;
        private readonly ClientRouter _clientRouter;

        public ClientInteractorFactory(ILogger<ClientInteractor> logger, ClientRouter clientRouter)
        {
            _logger = logger;
            _clientRouter = clientRouter;
        }

        public ClientInteractor Create(IAsyncEnumerable<ClientMessage> request,
            ChannelWriter<ServerMessage> response) => new(request, response, _logger, _clientRouter);
    }
}