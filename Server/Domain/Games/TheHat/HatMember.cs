using System.Threading.Tasks;
using Server.Application;
using Server.Application.ChainOfResponsibilityUtils;
using Server.Domain.Games.Meta;
using Shared.Protos;
using Shared.Protos.HatSharedClasses;

namespace Server.Domain.Games.TheHat
{
    public class HatMember
    {
        public IHatRole Role { get; }
        public HatMember(IChannelToClient<InGameServerMessage> clientInteractor, IHatRole role)
        {
            ClientInteractor = clientInteractor;
            Role = role;
        }

        public IChannelToClient<InGameServerMessage> ClientInteractor { get; }

        public Task HandleServerMessage(HatServerMessage message) => ClientInteractor.SendMessage(message);
    }
}