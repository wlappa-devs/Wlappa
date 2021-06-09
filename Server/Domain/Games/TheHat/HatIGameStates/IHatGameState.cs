using System.Threading.Tasks;
using Shared.Protos.HatSharedClasses;

namespace Server.Domain.Games.TheHat.HatIGameStates
{
    public interface IHatGameState
    {
        public Task<IHatGameState> HandleEvent(HatMember client, HatClientMessage e);
    }
}