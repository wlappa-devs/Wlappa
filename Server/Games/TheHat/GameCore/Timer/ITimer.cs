using Google.Protobuf.WellKnownTypes;
using Shared.Protos;
using Shared.Protos.HatSharedClasses;

namespace Server.Games.TheHat.GameCore.Timer
{
    public interface ITimer
    {
        void RequestEventIn(Duration duration, InGameClientMessage command);
    }
}