using Google.Protobuf.WellKnownTypes;
using Server.Games.TheHat.GameCore;
using Server.Games.TheHat.GameCore.Timer;

namespace TestServer.GameTests
{
    public class MockTimer: ITimer
    {
        public IStateCommand CommandToExecute { get; private set; }

        public void RequestEventIn(Duration duration, IStateCommand command)
        {
            CommandToExecute = command;
        }

        public IStateCommand CallBack()
        {
            var output = CommandToExecute;
            CommandToExecute = null;
            return output;
        }
    }
}