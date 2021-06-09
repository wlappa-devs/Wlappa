using System.Threading.Tasks;
using Server.Domain.Games.Meta;
using Shared.Protos.HatSharedClasses;

namespace Server.Domain.Games.TheHat
{
    public class HatMember
    {
        public IHatRole Role { get; }
        public HatMember(IInGameClientInteractor clientInteractor, IHatRole role)
        {
            ClientInteractor = clientInteractor;
            Role = role;
        }

        public IInGameClientInteractor ClientInteractor { get; }

        public Task HandleServerMessage(HatServerMessage message) => ClientInteractor.HandleInGameMessage(message);
    }
}