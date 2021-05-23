using System.Threading.Tasks;
using Server.Games.TheHat.GameCore;
using Shared.Protos.HatSharedClasses;

namespace Server.Games.TheHat.HatIGameStates
{
    public class ExplanationInProcess : IHatGameState
    {
        public Task<IHatGameState> HandleEvent(HatPlayer client, HatClientMessage e)
        {
            throw new System.NotImplementedException();
        }
    }
}