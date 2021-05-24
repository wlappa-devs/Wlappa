using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Server.Routing;
using Server.Routing.Helpers;
using Shared.Protos;

namespace Server.Games.Meta
{
    public class MulticastGroup
    {
        private readonly IReadOnlyCollection<IInGameClient> _players;

        public MulticastGroup(IReadOnlyCollection<IInGameClient> players)
        {
            _players = players;
        }

        public Task SendMulticastEvent(InGameServerMessage e) => 
            Task.WhenAll(_players.Select(player => player.HandleInGameMessage(e)));
    }
}