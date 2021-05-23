using System.Collections.Generic;
using Server.Routing;
using Server.Routing.Helpers;

namespace Server.Games.Meta
{
    public class MulticastGroup
    {
        private readonly IReadOnlyCollection<Client> _players;

        public MulticastGroup(IReadOnlyCollection<Client> players)
        {
            _players = players;
        }

        public async void SendMulticastEvent(InGameServerMessage e)
        {
            foreach (var player in _players)
            {
                await player.HandleInGameMessage(e);
            }
        }
    }
}