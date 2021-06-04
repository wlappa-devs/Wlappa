using System.Threading.Tasks;
using Server.Routing.Helpers;
using Shared.Protos.HatSharedClasses;

namespace Server.Games.TheHat
{
    public class HatMember
    {
        public IHatRole Role { get; }
        public HatMember(IInGameClient client, IHatRole role)
        {
            Client = client;
            Role = role;
        }

        public IInGameClient Client { get; }

        public Task HandleServerMessage(HatServerMessage message) =>
            Client.HandleInGameMessage(message);
    }
}