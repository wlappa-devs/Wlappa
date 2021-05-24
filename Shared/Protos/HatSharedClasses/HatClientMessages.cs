using System.Collections.Generic;
using System.Runtime.Serialization;
using ProtoBuf;

namespace Shared.Protos.HatSharedClasses
{
    [DataContract]
    [ProtoInclude(1, typeof(AddWords))]
    [ProtoInclude(2, typeof(ClientIsReady))]
    [ProtoInclude(3, typeof(GuessRight))]
    [ProtoInclude(4, typeof(TimerFinish))]
    public class HatClientMessage : InGameClientMessage
    {
    }

    [DataContract]
    public class AddWords : HatClientMessage
    {
        [ProtoMember(1)] public IReadOnlyList<string> Value { get; init; }
    }

    [DataContract]
    public class ClientIsReady : HatClientMessage { }
    
    [DataContract]
    public class GuessRight : HatClientMessage { }
    
    [DataContract]
    public class TimerFinish : HatClientMessage { }
}