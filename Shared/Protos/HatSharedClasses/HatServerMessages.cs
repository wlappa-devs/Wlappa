using System;
using System.Runtime.Serialization;
using ProtoBuf;

namespace Shared.Protos.HatSharedClasses
{
    [DataContract]
    [ProtoInclude(1, typeof(StartGame))]
    [ProtoInclude(2, typeof(AnnounceNextPair))]
    [ProtoInclude(3, typeof(ExplanationStarted))]
    [ProtoInclude(4, typeof(WordToGuess))]
    [ProtoInclude(5, typeof(TimeIsUp))]
    [ProtoInclude(6, typeof(WrongWordsAmount))]
    public class HatServerMessage : InGameServerMessage
    {
    }
    
    [DataContract]
    public class StartGame : HatServerMessage {}

    [DataContract]
    public class AnnounceNextPair : HatServerMessage
    {
        [ProtoMember(1)] public Guid Explainer { get; init; }
        [ProtoMember(1)] public Guid Understander { get; init; }
    }
    
    [DataContract]
    public class ExplanationStarted : HatServerMessage {}

    [DataContract]
    public class WordToGuess : HatServerMessage
    {
        [ProtoMember(1)] public string Value { get; init; }
    }
    
    [DataContract] 
    public class TimeIsUp : HatServerMessage {}
    
    [DataContract]
    public class WrongWordsAmount : HatServerMessage {}
}