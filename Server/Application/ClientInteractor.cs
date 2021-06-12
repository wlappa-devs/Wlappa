using System;
using System.Collections.Generic;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Server.Application.ChainOfResponsibilityUtils;
using Shared.Protos;

namespace Server.Application
{
    public class ClientInteractor : IChannelToClient<ServerMessage>
    {
        private readonly IAsyncEnumerable<ClientMessage> _requestStream;
        private readonly ChannelWriter<ServerMessage> _responseStream;
        private readonly ILogger<ClientInteractor> _logger;
        private readonly ClientRouter _clientRouter;
        private readonly ChainResolver _chainResolver;
        private readonly Func<ClientMessage, Task> _chain;
        public Guid Id { get; } = Guid.NewGuid();
        public string? Name { get; set; }

        public ClientInteractor(IAsyncEnumerable<ClientMessage> requestStream,
            ChannelWriter<ServerMessage> responseStream, ILogger<ClientInteractor> logger, ClientRouter clientRouter,
            ChainResolver chainResolver)
        {
            _requestStream = requestStream;
            _responseStream = responseStream;
            _logger = logger;
            _clientRouter = clientRouter;
            _chainResolver = chainResolver;
            _chain = chainResolver.GetChainForClient(Id);
        }

        public async Task StartProcessing()
        {
            await foreach (var message in _requestStream) await _chain(message);
        }

        public async void HandleConnectionFailure()
        {
            _logger.LogInformation("Got unstable disconnect");
            await _chain(new Disconnect());
            _chainResolver.RemoveAllHandlers(Id);
        }

        public async Task SendMessage(ServerMessage m) => await _responseStream.WriteAsync(m);

        public IChannelToClient<TNew> AsNewHandler<TNew>() where TNew : ServerMessage => this;
    }
}