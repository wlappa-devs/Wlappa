using System;
using Shared.Protos;

namespace Server.Domain.Games.Meta
{
    public interface ITimer
    {
        void RequestEventIn(TimeSpan duration, InGameClientMessage command, Guid guid);

        void CancelEvent(Guid eventId);
    }
}