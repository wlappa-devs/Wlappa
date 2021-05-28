using System;
using System.Threading.Tasks;
using Server.Games.TheHat.GameCore;
using Shared.Protos.HatSharedClasses;

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

        public async Task<IHatGameState> HandleEvent(HatMember client, HatClientMessage e)
        {
            switch (e)
            {
                case HatGuessRight:
                    if (_game.IsManged)
                    {
                        if (client != _game.Manger)
                            return this;
                    }
                    else if (client is HatPlayer player)
                        if (player != _game.Explainer)
                            return this;

                    _game.GuessCurrentWord();
                    await _game.AnnounceScores();
                    await _game.TellTheWord(new HatWordToGuess {Value = _game.TakeWord()?.Value});
                    _alreadyGuessed++;
                    return this;
                case HatTimerFinish:
                    // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                    if (client is not null)
                        return this;
                    
                    await _game.SendMulticastMessage(new HatTimeIsUp());
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