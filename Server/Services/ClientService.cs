using System.Collections.Generic;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ProtoBuf.Grpc;
using Server.Application;
using Server.Domain.ChainOfResponsibilityUtils;
using Shared.Protos;

namespace Server.Services
{
    public class ClientService : IMainServiceContract
    {
        // ReSharper disable once NotAccessedField.Local
        private readonly ILogger<ClientService> _logger;

        private readonly ClientInteractorFactory _clientInteractorFactory;
        private readonly PreGameClientFactory _preGameClientFactory;
        private readonly ISubscriptionManager<PreGameClientMessage> _subscriptionManager;

        public ClientService(ILogger<ClientService> logger, ClientInteractorFactory clientInteractorFactory,
            PreGameClientFactory preGameClientFactory, ISubscriptionManager<PreGameClientMessage> subscriptionManager)
        {
            _logger = logger;
            _clientInteractorFactory = clientInteractorFactory;
            _preGameClientFactory = preGameClientFactory;
            _subscriptionManager = subscriptionManager;
        }

        public IAsyncEnumerable<ServerMessage> Connect(
            IAsyncEnumerable<ClientMessage> request, CallContext context = default)
        {
            var toClientChannel = Channel.CreateUnbounded<ServerMessage>();
#pragma warning disable 4014
            HandleClient(request, toClientChannel.Writer, context);
#pragma warning restore 4014
            return toClientChannel.Reader.ReadAllAsync();
        }

        private async Task HandleClient(IAsyncEnumerable<ClientMessage> request,
            ChannelWriter<ServerMessage> response, CallContext context = default)
        {
            var clientInteractor = _clientInteractorFactory.Create(request, response);
            var preGameClient = _preGameClientFactory.Create(clientInteractor);
            _subscriptionManager.SubscribeForClient(clientInteractor.Id, preGameClient);
            context.CancellationToken.Register(() => clientInteractor.HandleConnectionFailure());
            await clientInteractor.StartProcessing();
            _subscriptionManager.UnsubscribeFromClient(clientInteractor.Id);
            response.TryComplete();
        }
    }
}