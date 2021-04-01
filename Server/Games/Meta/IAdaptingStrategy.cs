using System.Collections.Generic;
using Server.Routing;

namespace Server.Games.Meta
{
    public interface IAdaptingStrategy
    {
        string? ValidateRolesWithConfig(IGameConfiguration config, GameCreationPayload payload);
        IGameFactory Factory { get; }
        GameType Type { get; }
    }
}