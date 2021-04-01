using System;
using System.Collections.Generic;
using System.Linq;
using Server.Games.Meta;

namespace Server.Games.Clicker
{
    public class ClickGame : IGame
    {
        private readonly ClickGameConfiguration _config;
        private long _currentValue;
        private readonly MulticastGroup _allPlayers;

        public ClickGame(ClickGameConfiguration config, GameCreationPayload payload)
        {
            _config = config;
            IReadOnlyCollection<IPlayer> players = payload.PlayerToRole.Keys.ToArray();
            _allPlayers = new MulticastGroup(players);
        }

        public void HandleEvent(IGameEvent e)
        {
            switch (e)
            {
                case IncrementEvent:
                    _currentValue += _config.IncrementValue;
                    _allPlayers.SendMulticastEvent(new NewValueEvent()
                    {
                        Value = _currentValue
                    });
                    break;
                default:
                    throw new ArgumentException("Unsupported event");
            }
        }
    }

    public class ClickGameFactory : IGameFactory, IGameFactoryMark<ClickGame>
    {
        public IGame Create(IGameConfiguration config, GameCreationPayload payload)
        {
            if (config is not ClickGameConfiguration correctConfig)
                throw new ArgumentException("Provided invalid configuration");
            return new ClickGame(correctConfig, payload);
        }
        
        public IReadOnlyList<string> Roles { get; } = new[] {"Clicker"};
    }
}