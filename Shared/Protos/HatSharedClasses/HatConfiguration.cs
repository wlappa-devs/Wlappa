using System.Runtime.Serialization;
using Google.Protobuf.WellKnownTypes;
using ProtoBuf;

namespace Shared.Protos.HatSharedClasses
{
    [DataContract]
    public class HatConfiguration : GameConfiguration
    {
        [ProtoMember(1)] public Duration TimeToExplain { get; init; }

        [ProtoMember(2)] public HatGameModeConfiguration HatGameModeConfiguration { get; init; }

        [ProtoMember(3)] public int WordsToBeWritten { get; init; }
    }

    [DataContract]
    [ProtoInclude(1, typeof(HatPairChoosingModeConfiguration))]
    [ProtoInclude(2, typeof(HatCircleChoosingModeConfiguration))]
    public abstract class HatGameModeConfiguration
    {
        public abstract bool GameIsOver(int lapCount, int explainerIndex, int playersCount);
    }

    [DataContract]
    public class HatPairChoosingModeConfiguration : HatGameModeConfiguration
    {
        [ProtoMember(1)] public int NumberOfLapsToPlay { get; init; }

        public override bool GameIsOver(int lapCount, int explainerIndex, int playersCount) =>
            lapCount == NumberOfLapsToPlay && explainerIndex == playersCount - 1;
    }

    [DataContract]
    public class HatCircleChoosingModeConfiguration : HatGameModeConfiguration
    {
        public override bool GameIsOver(int lapCount, int explainerIndex, int playersCount) =>
            lapCount == playersCount - 1 && explainerIndex == playersCount - 1;
    }
}