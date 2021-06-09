using System;
using Google.Protobuf.WellKnownTypes;
using Server.Domain.Games.Meta;
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

        public void RequestEventIn(TimeSpan duration, InGameClientMessage command, Guid eventId)
        {
            commandToExecute = command;
        }

        public void CancelEvent(Guid eventId)
        {
            // TODO Mock timer event cancellation;
        }
    }
}