using System;
using System.Threading.Tasks;
using Server.Games.TheHat.GameCore;
using Shared.Protos.HatSharedClasses;

namespace Server.Games.TheHat.HatIGameStates
{
    public class WaitingForPLayersToGetReady : IHatGameState
    {
        private readonly bool _explainerIsReady;
        private readonly bool _understanderIsReady;
        private readonly HatIGame _game;

        public WaitingForPLayersToGetReady(bool explainerIsReady, bool understanderIsReady, HatIGame game)
        {
            _explainerIsReady = explainerIsReady;
            _understanderIsReady = understanderIsReady;
            _game = game;
        }

        public async Task<IHatGameState> HandleEvent(HatPlayer client, HatClientMessage e)
        {
            if (e is ClientIsReady)
            {
                if (client.Id == _game.CurrentPair.explainerIndex)
                {
                    if (!_understanderIsReady)
                        return new WaitingForPLayersToGetReady(true, false, _game);
                    return BothAreReady();
                }

                if (client.Id == _game.CurrentPair.understanderIndex)
                {
                    if (!_explainerIsReady)
                        return new WaitingForPLayersToGetReady(false, true, _game);
                    return BothAreReady();
                }
            }

            throw new ArgumentOutOfRangeException(nameof(e), "Unexpected command");
        }

        private IHatGameState BothAreReady()
        {
            _game.SetTimerForExplanation();
            _game.TellToExplainer(new WordToGuess {Value = _game.TakeWord()?.Value});
            return new ExplanationInProcess();
        }
    }
}