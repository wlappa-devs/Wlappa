using System.Runtime.Serialization;
using ProtoBuf;

namespace Shared.Protos
{
    [DataContract]
    public class ClickGameConfiguration : GameConfiguration
    {
        [ProtoMember(1)] public int IncrementValue { get; init; }
        [ProtoMember(2)] public int ClicksToWin { get; init; }
    }

    [DataContract]
    [ProtoInclude(1, typeof(NewValueEvent))]
    public class ClickerServerMessage : InGameServerMessage
    {
    }

    [DataContract]
    public class NewValueEvent : ClickerServerMessage
    {
        [ProtoMember(1)] public long Value { get; init; }
    }

    [DataContract]
    [ProtoInclude(1, typeof(IncrementEvent))]
    public class ClickerClientMessage : InGameClientMessage
    {
    }

    [DataContract]
    public class IncrementEvent : ClickerClientMessage
    {
    }
}