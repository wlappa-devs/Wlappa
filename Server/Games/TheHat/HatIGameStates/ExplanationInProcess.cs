using System;
using System.Threading.Tasks;
using Server.Games.TheHat.GameCore;
using Shared.Protos.HatSharedClasses;
using GuessRight = Shared.Protos.HatSharedClasses.GuessRight;

namespace Server.Games.TheHat.HatIGameStates
{
    public class ExplanationInProcess : IHatGameState
    {
        private readonly HatIGame _game;
        private int _alreadyGuessed = 0;

        public ExplanationInProcess(HatIGame game)
        {
            _game = game;
        }

        public async Task<IHatGameState> HandleEvent(HatPlayer client, HatClientMessage e)
        {
            switch (e)
            {
                case GuessRight:
                    _game.GuessCurrentWord();
                    await _game.TellToExplainer(new WordToGuess {Value = _game.TakeWord()?.Value});
                    _alreadyGuessed++;
                    return this;
                case TimerFinish:
                    await _game.SendMulticastMessage(new TimeIsUp());
                    _game.ReturnCurrentWordInHatIfNeeded();
                    _game.MoveToNextPair();
                    await _game.AnnounceCurrentPair();
                    return new WaitingForPLayersToGetReady(false, false, _game);
                default:
                    throw new ArgumentOutOfRangeException(nameof(e), "Unexpected command");
            }
        }
    }
}