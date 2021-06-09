using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shared.Protos;

namespace Server.Domain.Games.Meta
{
    public class Timer : ITimer
    {
        private readonly HashSet<Guid> _set = new();
        public Game? Game { private get; set; }

        public async void RequestEventIn(TimeSpan duration, InGameClientMessage command, Guid eventId)
        {
            _set.Add(eventId);
            await Task.Delay(duration);
            if (Game is null || !_set.Contains(eventId)) return;
            _set.Remove(eventId);
            await Game.HandleEvent(null, command);
        }

        public void CancelEvent(Guid eventId) => _set.Remove(eventId);
    }
}