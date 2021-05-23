using System.Runtime.Serialization;
using Google.Protobuf.WellKnownTypes;
using ProtoBuf;

namespace Shared.Protos.HatSharedClasses
{
    [DataContract]
    public class HatConfiguration : GameConfiguration
    {
        [ProtoMember(1)] public Duration TimeToExplain { get; init; }
        
        [ProtoMember(2)] public PairChoosingMode PairChoosingMode { get; init; }
        
        [ProtoMember(3)] public int WordsToBeWritten { get; init; }
    }
}