using System;

namespace Server.Games.Meta
{
    public interface IPlayer
    {
        long Id { get; }
        string Name { get; }

        void HandleEvent(IToPlayerEvent e);
    }

    public interface IToPlayerEvent
    {
    }

    public interface IPlayerConnection
    {
        void AddEventListener(Action<IPlayerEvent> listener);
    }
}