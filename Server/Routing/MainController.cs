using System;
using System.Collections.Generic;
using Server.Games.Meta;

namespace Server.Routing
{
    public class MainController
    {
        private static Dictionary<Guid, GameController> _games = new();

        public void Connect(IPlayer player, IPlayerConnection connection, IInitialMessage initialMessage,
            IAdaptingStrategy strategy)
        {
            switch (initialMessage)
            {
                case GameCreate e:
                    var newGameId = Guid.NewGuid();
                    _games[newGameId] = new GameController(strategy.Factory, e.Configuration, player.Id, strategy.Type);
                    _games[newGameId].ConnectPlayer(player, connection);
                    break;
                case GameJoin e:
                    _games[Guid.Parse(e.GameCode)].ConnectPlayer(player, connection);
                    break;
            }
        }

        public void CreateGame(IPlayer player, IPlayerConnection connection, GameCreate initialMessage)
        {
        }

        public GameType GetGameTypeForCode(Guid code) => _games[code].Type;
    }
}