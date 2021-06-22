using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Server.Domain.ChainOfResponsibilityUtils;
using Shared.Protos;

namespace Server.Domain.Games.Meta
{
    public class MulticastGroup
    {
        private readonly IReadOnlyCollection<IChannelToClient<InGameServerMessage>> _players;

        public MulticastGroup(IReadOnlyCollection<IChannelToClient<InGameServerMessage>> players)
        {
            _players = players;
        }

        public Task SendMulticastEvent(InGameServerMessage e) =>
            Task.WhenAll(_players.Select(player => player.SendMessage(e)));
    }
}