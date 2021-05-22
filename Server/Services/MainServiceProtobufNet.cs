using System.Collections.Generic;
using ProtoBuf.Grpc;
using Server.Routing;

namespace Server.Services
{
    public class MainServiceProtobufNet : IMainServiceContract
    {
        public async IAsyncEnumerable<ServerMessageProtobufNet> Connect(
            IAsyncEnumerable<ClientMessageProtobufNet> request, CallContext context = default)
        {
            await foreach (var e in request)
            {
                yield return new ServerMessageProtobufNet
                {
                    Data = e.Data
                };
            }
        }
    }
}