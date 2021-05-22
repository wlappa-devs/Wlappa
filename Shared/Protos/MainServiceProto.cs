using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;
using ProtoBuf;
using ProtoBuf.Grpc;

namespace Server.Routing
{
    [ServiceContract(Name = "MainServiceProtobufNet")]
    public interface IMainServiceContract
    {
        IAsyncEnumerable<ServerMessageProtobufNet> Connect(IAsyncEnumerable<ClientMessageProtobufNet> request,
            CallContext context = default);
    }

    [DataContract]
    public class ClientMessageProtobufNet
    {
        [ProtoMember(1)]
        public string Data;
    }

    [DataContract]
    public class ServerMessageProtobufNet
    {
        [ProtoMember(1)]
        public string Data;
    }
}