using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shared.Protos;

namespace Server.Domain.Games.Meta
{
    public class MulticastGroup
    {
        private readonly IReadOnlyCollection<IInGameClientInteractor> _players;

        public MulticastGroup(IReadOnlyCollection<IInGameClientInteractor> players)
        {
            _players = players;
        }

        public Task SendMulticastEvent(InGameServerMessage e) => 
            Task.WhenAll(_players.Select(player => player.HandleInGameMessage(e)));
    }
}