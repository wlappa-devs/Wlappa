using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using ProtoBuf.Grpc;
using Server.Routing;
using Server.Routing.Helpers;
using Shared.Protos;

namespace Server.Services
{
    public class MainServiceProtobufNet : IMainServiceContract
    {
        private readonly ILogger<MainServiceProtobufNet> _logger;

        private readonly ClientFactory _clientFactory;

        public MainServiceProtobufNet(ILogger<MainServiceProtobufNet> logger, ClientFactory clientFactory)
        {
            _logger = logger;
            _clientFactory = clientFactory;
        }

        public IAsyncEnumerable<ServerMessage> Connect(
            IAsyncEnumerable<ClientMessage> request, CallContext context = default)
        {
            var toClientChannel = Channel.CreateUnbounded<ServerMessage>();
            HandleClient(request, toClientChannel.Writer, context);
            return toClientChannel.Reader.ReadAllAsync();
        }

        private async Task HandleClient(IAsyncEnumerable<ClientMessage> request,
            ChannelWriter<ServerMessage> response, CallContext context = default)
        {
            var client = _clientFactory.Create(request, response);
            context.CancellationToken.Register(() => client.HandleConnectionFailure());
            await client.StartProcessing();

            response.TryComplete();
        }
    }
}