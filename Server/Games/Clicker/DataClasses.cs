using Server.Games.Meta;

namespace Server.Games.Clicker
{
    public class ClickGameConfiguration : IGameConfiguration
    {
        public ClickGameConfiguration(int incrementValue)
        {
            IncrementValue = incrementValue;
        }

        public int IncrementValue { get; }
    }

    public class IncrementEvent : IPlayerEvent
    {
        public IncrementEvent(IPlayer player)
        {
            Player = player;
        }

        public IPlayer Player { get; }
    }

    public class NewValueEvent : IToPlayerEvent
    {
        public long Value { get; init; }
    }
}