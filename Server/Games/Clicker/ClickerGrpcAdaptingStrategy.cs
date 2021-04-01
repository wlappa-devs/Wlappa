using System;
using GrpcService;
using Server.Games.Meta;
using Server.Routing;

namespace Server.Games.Clicker
{
    public class ClickerGrpcAdaptingStrategy : ClickerAdaptingStrategy, IGrpcAdaptingStrategy
    {
        public GameCreate ParseConfigurationMessage(Configuration message)
        {
            return new GameCreate(new ClickGameConfiguration(message.ClickGameConfiguration.IncrementValue),
                GameType.Clicker);
        }

        public IPlayerEvent ParsePlayerEvent(GameEvent gameEvent, IPlayer player)
        {
            if (gameEvent.ClickGameEvent.EventCase == ClickGameEvent.EventOneofCase.IncrementEvent)
                return new IncrementEvent(player);
            throw new ArgumentException("Unsupported event", nameof(gameEvent));
        }
    }
}