using System;
using System.Runtime.Serialization;
using ProtoBuf;

namespace Shared.Protos.HatSharedClasses
{
    [DataContract]
    [ProtoInclude(1, typeof(HatStartGame))]
    [ProtoInclude(2, typeof(HatAnnounceNextPair))]
    [ProtoInclude(3, typeof(HatExplanationStarted))]
    [ProtoInclude(4, typeof(HatWordToGuess))]
    [ProtoInclude(5, typeof(HatTimeIsUp))]
    [ProtoInclude(6, typeof(HatInvalidWordsSet))]
    [ProtoInclude(7, typeof(HatFinishMessage))]
    [ProtoInclude(8, typeof(HatPointsUpdated))]
    public abstract class HatServerMessage : InGameServerMessage
    {
    }

    [DataContract]
    public class HatStartGame : HatServerMessage
    {
    }

    [DataContract]
    public class HatAnnounceNextPair : HatServerMessage
    {
        [ProtoMember(1)] public Guid Explainer { get; init; }
        [ProtoMember(2)] public Guid Understander { get; init; }
    }

    [DataContract]
    public class HatExplanationStarted : HatServerMessage
    {
    }

    [DataContract]
    public class HatWordToGuess : HatServerMessage
    {
        [ProtoMember(1)] public string Value { get; init; }
    }

    [DataContract]
    public class HatTimeIsUp : HatServerMessage
    {
    }

    [DataContract]
    public class HatInvalidWordsSet : HatServerMessage
    {
    }

    [DataContract]
    public class HatPointsUpdated : HatServerMessage
    {
        
    }

    [DataContract]
    [ProtoInclude(1, typeof(HatNoWordsLeft))]
    [ProtoInclude(2, typeof(HatRotationFinished))]
    public abstract class HatFinishMessage : HatServerMessage
    {
    }

    [DataContract]
    public class HatNoWordsLeft : HatFinishMessage
    {
    }

    [DataContract]
    public class HatRotationFinished : HatFinishMessage
    {
    }
}