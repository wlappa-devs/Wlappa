using Google.Protobuf.WellKnownTypes;

namespace Server.Games.TheHat.GameCore.Timer
{
    public interface ITimer
    {
        void RequestEventIn(Duration duration, IStateCommand command);
    }
}