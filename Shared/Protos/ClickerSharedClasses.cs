using System.Runtime.Serialization;
using ProtoBuf;

namespace Shared.Protos
{
    [DataContract]
    [ProtoContract]
    public class ClickGameConfiguration : GameConfiguration
    {
        [ProtoMember(1)] public int IncrementValue { get; set; }
        [ProtoMember(2)] public int ClicksToWin { get; set; }
    }

    [DataContract]
    [ProtoContract]
    [ProtoInclude(1, typeof(ClickerNewValueEvent))]
    public class ClickerServerMessage : InGameServerMessage
    {
    }

    [DataContract]
    [ProtoContract]
    public class ClickerNewValueEvent : ClickerServerMessage
    {
        [ProtoMember(1)] public long Value { get; set; }
    }

    [DataContract]
    [ProtoContract]
    [ProtoInclude(1, typeof(ClickerIncrementEvent))]
    [ProtoInclude(2, typeof(ClickerTimePassedEvent))]
    public class ClickerClientMessage : InGameClientMessage
    {
    }

    [ProtoContract]
    [DataContract]
    public class ClickerIncrementEvent : ClickerClientMessage
    {
    }

    [ProtoContract]
    [DataContract]
    public class ClickerTimePassedEvent : ClickerClientMessage
    {
    }
}