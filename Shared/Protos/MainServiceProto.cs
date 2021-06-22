using System.Collections.Generic;
using System.ServiceModel;
using ProtoBuf.Grpc;

namespace Shared.Protos
{
    [ServiceContract(Name = "ClientService", Namespace = "Wlappa")]
    // ReSharper disable once ServiceContractWithoutOperations
    public interface IMainServiceContract
    {
        // ReSharper disable once UnusedMemberInSuper.Global
        IAsyncEnumerable<ServerMessage> Connect(IAsyncEnumerable<ClientMessage> request,
            CallContext context = default);
    }
}