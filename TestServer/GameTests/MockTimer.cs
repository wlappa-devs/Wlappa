using Google.Protobuf.WellKnownTypes;
using Server.Games.TheHat.GameCore.Timer;
using Shared.Protos;

namespace TestServer.GameTests
{
    public class MockTimer: ITimer
    {
        private InGameClientMessage commandToExecute;

        public InGameClientMessage CallBack()
        {
            var output = commandToExecute;
            commandToExecute = null;
            return output;
        }

        public void RequestEventIn(Duration duration, InGameClientMessage command)
        {
            commandToExecute = command;
        }
    }
}