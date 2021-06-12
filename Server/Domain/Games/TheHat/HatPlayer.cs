using Server.Application;
using Server.Application.ChainOfResponsibilityUtils;
using Server.Domain.Games.Meta;
using Shared.Protos;
using Shared.Protos.HatSharedClasses;

namespace Server.Domain.Games.TheHat
{
    public class HatPlayer : HatMember
    {
        public int Score { get; private set; }
        public int Id { get; }

        public HatPlayer(IChannelToClient<InGameServerMessage> clientInteractor, int id, IHatRole role) : base(
            clientInteractor, role)
        {
            Id = id;
            Score = 0;
        }

        public int IncrementScore() => ++Score;
        public int DecrementScore() => Score == 0 ? 0 : --Score;
    }
}