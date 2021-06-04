using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
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
                case HatCancelExplanation:
                    _game.CancelTimer();
                    _game.ReturnCurrentWordInHatIfNeeded();
                    await MoveNextAndAnnounce();
                    return new WaitingForPLayersToGetReady(false, false, _game);
                case HatGuessRight:
                    if (_game.IsManged)
                    {
                        if (client != _game.Manger)
                        {
                            _game.Logger.Log(LogLevel.Information, "Not manager sent GuessRight");
                            return this;
                        }

                        _game.Logger.Log(LogLevel.Information, "Manager sent GuessRight");
                    }
                    else if (client is HatPlayer player)
                    {
                        _game.Logger.Log(LogLevel.Information, "Unmanaged GuessRight");
                        if (player != _game.Explainer)
                            return this;
                    }

                    _game.GuessCurrentWord();
                    await _game.AnnounceScores();
                    await _game.TellTheWord(new HatWordToGuess {Value = (await _game.TakeWord())?.Value});
                    _alreadyGuessed++;
                    return this;
                case HatTimerFinish:
                    // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                    if (client is not null)
                        return this;

                    await _game.SendMulticastMessage(new HatTimeIsUp());
                    _game.ReturnCurrentWordInHatIfNeeded();
                    await MoveNextAndAnnounce();
                    return new WaitingForPLayersToGetReady(false, false, _game);
                default:
                    throw new ArgumentOutOfRangeException(nameof(e), "Unexpected command");
            }
        }

        private async Task MoveNextAndAnnounce()
        {
            _game.MoveToNextPair();
            await _game.AnnounceCurrentPair();
        }
    }
}