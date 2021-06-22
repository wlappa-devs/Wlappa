using System;
using Server.Domain.Games.Meta;
using Shared.Protos;

namespace TestServer.GameTests
{
    public class MockTimer : ITimer
    {
        public void RequestEventIn(TimeSpan ___, InGameClientMessage _, Guid __)
        {
        }

        public void CancelEvent(Guid eventId)
        {
        }
    }
}