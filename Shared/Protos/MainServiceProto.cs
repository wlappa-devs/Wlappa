using System.Collections.Generic;
using System.ServiceModel;
using ProtoBuf.Grpc;

namespace Server.Routing
{
    [ServiceContract(Name = "MainServiceProtobufNet")]
    public interface IMainServiceContract
    {
        IAsyncEnumerable<ServerMessage> Connect(IAsyncEnumerable<ClientMessage> request,
            CallContext context = default);
    }
}