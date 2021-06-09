using System.Collections.Generic;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ProtoBuf.Grpc;
using Server.Application;
using Shared.Protos;

namespace Server.Services
{
    public class MainServiceProtobufNet : IMainServiceContract
    {
        // ReSharper disable once NotAccessedField.Local
        private readonly ILogger<MainServiceProtobufNet> _logger;

        private readonly ClientInteractorFactory _clientInteractorFactory;

        public MainServiceProtobufNet(ILogger<MainServiceProtobufNet> logger, ClientInteractorFactory clientInteractorFactory)
        {
            _logger = logger;
            _clientInteractorFactory = clientInteractorFactory;
        }

        public IAsyncEnumerable<ServerMessage> Connect(
            IAsyncEnumerable<ClientMessage> request, CallContext context = default)
        {
            var toClientChannel = Channel.CreateUnbounded<ServerMessage>();
#pragma warning disable 4014
            HandleClient(request, toClientChannel.Writer, context); // TODO consult
#pragma warning restore 4014
            return toClientChannel.Reader.ReadAllAsync();
        }

        private async Task HandleClient(IAsyncEnumerable<ClientMessage> request,
            ChannelWriter<ServerMessage> response, CallContext context = default)
        {
            var client = _clientInteractorFactory.Create(request, response);
            context.CancellationToken.Register(() => client.HandleConnectionFailure());
            await client.StartProcessing();

            response.TryComplete();
        }
    }
}