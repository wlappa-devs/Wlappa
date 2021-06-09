using System.Collections.Generic;
using System.Runtime.Serialization;
using ProtoBuf;

namespace Shared.Protos.HatSharedClasses
{
    [DataContract]
    [ProtoContract]
    [ProtoInclude(1, typeof(HatAddWords))]
    [ProtoInclude(2, typeof(HatClientIsReady))]
    [ProtoInclude(3, typeof(HatGuessRight))]
    [ProtoInclude(4, typeof(HatTimerFinish))]
    [ProtoInclude(5, typeof(HatCancelExplanation))]
    public class HatClientMessage : InGameClientMessage
    {
    }

    [DataContract]
    [ProtoContract]
    public class HatAddWords : HatClientMessage
    {
        [ProtoMember(1)] public IReadOnlyList<string> Value { get; set; }
    }

    [DataContract]
    [ProtoContract]
    public class HatClientIsReady : HatClientMessage { }
    
    [DataContract]
    [ProtoContract]
    public class HatGuessRight : HatClientMessage { }
    
    [DataContract]
    [ProtoContract]
    public class HatTimerFinish : HatClientMessage { }

    [DataContract]
    [ProtoContract]
    public class HatCancelExplanation : HatClientMessage { }
}