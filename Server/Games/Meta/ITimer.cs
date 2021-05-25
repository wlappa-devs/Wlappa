using System;
using Shared.Protos;

namespace Server.Games.Meta
{
    public interface ITimer
    {
        void RequestEventIn(TimeSpan duration, InGameClientMessage command, Guid guid);

        void CancelEvent(Guid eventId);
    }
}