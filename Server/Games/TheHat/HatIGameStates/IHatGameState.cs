using System.Threading.Tasks;
using Server.Games.TheHat.GameCore;
using Shared.Protos.HatSharedClasses;

namespace Server.Games.TheHat.HatIGameStates
{
    public interface IHatGameState
    {
        public Task<IHatGameState> HandleEvent(HatMember client, HatClientMessage e);
    }
}