using System.Collections.Generic;
using System.Runtime.Serialization;
using ProtoBuf;

namespace Shared.Protos.HatSharedClasses
{
    [DataContract]
    [ProtoContract]
    [ProtoInclude(1, typeof(AddWords))]
    [ProtoInclude(2, typeof(ClientIsReady))]
    [ProtoInclude(3, typeof(GuessRight))]
    [ProtoInclude(4, typeof(TimerFinish))]
    public class HatClientMessage : InGameClientMessage
    {
    }

    [DataContract]
    [ProtoContract]
    public class AddWords : HatClientMessage
    {
        [ProtoMember(1)] public IReadOnlyList<string> Value { get; init; }
    }

    [DataContract]
    [ProtoContract]
    public class ClientIsReady : HatClientMessage { }
    
    [DataContract]
    [ProtoContract]
    public class GuessRight : HatClientMessage { }
    
    [DataContract]
    [ProtoContract]
    public class TimerFinish : HatClientMessage { }
}