using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using ProtoBuf;

namespace Shared.Protos.HatSharedClasses
{
    [DataContract]
    [ProtoContract]
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
    [ProtoContract]
    public class HatStartGame : HatServerMessage
    {
    }

    [DataContract]
    [ProtoContract]
    public class HatAnnounceNextPair : HatServerMessage
    {
        [ProtoMember(1)] public Guid Explainer { get; init; }
        [ProtoMember(2)] public Guid Understander { get; init; }
    }

    [DataContract]
    [ProtoContract]
    public class HatExplanationStarted : HatServerMessage
    {
    }

    [DataContract]
    [ProtoContract]
    public class HatWordToGuess : HatServerMessage
    {
        [ProtoMember(1)] public string Value { get; init; }
    }

    [DataContract]
    [ProtoContract]
    public class HatTimeIsUp : HatServerMessage
    {
    }

    [DataContract]
    [ProtoContract]
    public class HatInvalidWordsSet : HatServerMessage
    {
    }

    [DataContract]
    [ProtoContract]
    public class HatPointsUpdated : HatServerMessage
    {
        [ProtoMember(1)] public Dictionary<Guid, int> GuidToPoints { get; init; }

    }

    [DataContract]
    [ProtoContract]
    [ProtoInclude(1, typeof(HatNoWordsLeft))]
    [ProtoInclude(2, typeof(HatRotationFinished))]
    public abstract class HatFinishMessage : HatServerMessage
    {
    }

    [DataContract]
    [ProtoContract]
    public class HatNoWordsLeft : HatFinishMessage
    {
    }

    [DataContract]
    [ProtoContract]
    public class HatRotationFinished : HatFinishMessage
    {
    }
}