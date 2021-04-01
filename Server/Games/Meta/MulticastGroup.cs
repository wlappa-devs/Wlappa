using System.Collections.Generic;

namespace Server.Games.Meta
{
    public class MulticastGroup
    {
        private readonly IReadOnlyCollection<IPlayer> _players;
        public MulticastGroup(IReadOnlyCollection<IPlayer> players)
        {
            _players = players;
        }

        public void SendMulticastEvent(IToPlayerEvent e)
        {
            foreach (var player in _players)
            {
                player.HandleEvent(e);
            }
        }
    }
}