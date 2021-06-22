using Server.Domain.ChainOfResponsibilityUtils;
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

        // ReSharper disable once UnusedMethodReturnValue.Global
        public int IncrementScore() => ++Score;
        // ReSharper disable once UnusedMember.Global
        public int DecrementScore() => Score == 0 ? 0 : --Score;
    }
}