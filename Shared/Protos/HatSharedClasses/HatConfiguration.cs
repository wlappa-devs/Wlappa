using System.Runtime.Serialization;
using ProtoBuf;
using ProtoBuf.WellKnownTypes;

namespace Shared.Protos.HatSharedClasses
{
    [DataContract]
    [ProtoContract]
    public class HatConfiguration : GameConfiguration
    {
        [ProtoMember(1)] public Duration TimeToExplain { get; set; }

        [ProtoMember(2)] public HatGameModeConfiguration HatGameModeConfiguration { get; set; }

        [ProtoMember(3)] public int WordsToBeWritten { get; set; }
    }

    [DataContract]
    [ProtoContract]
    [ProtoInclude(1, typeof(HatPairChoosingModeConfiguration))]
    [ProtoInclude(2, typeof(HatCircleChoosingModeConfiguration))]
    public abstract class HatGameModeConfiguration
    {
        public abstract bool GameIsOver(int lapCount, int explainerIndex, int playersCount);
    }

    [DataContract]
    [ProtoContract]
    public class HatPairChoosingModeConfiguration : HatGameModeConfiguration
    {
        [ProtoMember(1)] public int NumberOfLapsToPlay { get; set; }

        public override bool GameIsOver(int lapCount, int explainerIndex, int playersCount) =>
            lapCount == NumberOfLapsToPlay && explainerIndex == playersCount - 1;
    }

    [DataContract]
    [ProtoContract]
    public class HatCircleChoosingModeConfiguration : HatGameModeConfiguration
    {
        public override bool GameIsOver(int lapCount, int explainerIndex, int playersCount) =>
            lapCount == playersCount - 1 && explainerIndex == playersCount - 1;
    }
}