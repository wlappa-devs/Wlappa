using System.Threading.Tasks;
using Server.Routing.Helpers;
using Shared.Protos.HatSharedClasses;

namespace Server.Games.TheHat.GameCore
{
    public class HatPlayer : HatMember
    {
        public int Score { get; private set; }
        public int Id { get; }

        public HatPlayer(IInGameClient client, int id, HatRole role) : base(client, role)
        {
            Id = id;
            Score = 0;
        }

        public int IncrementScore() => ++Score;
        public int DecrementScore() => Score == 0 ? 0 : --Score;
    }
}