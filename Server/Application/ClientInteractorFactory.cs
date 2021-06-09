using System.Collections.Generic;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using Shared.Protos;

namespace Server.Application
{
    public class ClientInteractorFactory
    {
        private readonly ILogger<ClientInteractor> _logger;
        private readonly MainController _mainController;

        public ClientInteractorFactory(ILogger<ClientInteractor> logger, MainController mainController)
        {
            _logger = logger;
            _mainController = mainController;
        }

        public ClientInteractor Create(IAsyncEnumerable<ClientMessage> request,
            ChannelWriter<ServerMessage> response) => new(request, response, _logger, _mainController);
    }
}