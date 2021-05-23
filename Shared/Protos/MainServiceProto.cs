using System.Collections.Generic;
using System.ServiceModel;
using ProtoBuf.Grpc;

namespace Shared.Protos
{
    [ServiceContract(Name = "MainServiceProtobufNet")]
    public interface IMainServiceContract
    {
        IAsyncEnumerable<ServerMessage> Connect(IAsyncEnumerable<ClientMessage> request,
            CallContext context = default);
    }
}