using System.Collections.Generic;
using Server.Games.Meta;
using Server.Routing;

namespace Server.Games.Clicker
{
    public class ClickerAdaptingStrategy : IAdaptingStrategy
    {
        public string? ValidateRolesWithConfig(IGameConfiguration config, GameCreationPayload payload)
        {
            return null;
        }

        public IGameFactory Factory { get; } = new ClickGameFactory();
        public GameType Type { get; } = GameType.Clicker;

    }
}