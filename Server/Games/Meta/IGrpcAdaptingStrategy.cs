using System;
using GrpcService;
using Server.Games.Clicker;
using Server.Routing;

namespace Server.Games.Meta
{
    public interface IGrpcAdaptingStrategy : IAdaptingStrategy
    {
        GameCreate ParseConfigurationMessage(Configuration message);
        IPlayerEvent ParsePlayerEvent(GameEvent gameEvent, IPlayer player);
    }
}