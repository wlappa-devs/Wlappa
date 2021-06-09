using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Shared.Protos.HatSharedClasses;

namespace Server.Domain.Games.TheHat.HatIGameStates
{
    public class ExplanationInProcess : IHatGameState
    {
        private readonly HatGame _game;

        public ExplanationInProcess(HatGame game)
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
                    if (client.ClientInteractor.Id != _game.ExplanationControllingMember.ClientInteractor.Id)
                    {
                        _game.Logger.LogError("Unauthorized GuessRight");
                        return this;
                    }

                    _game.GuessCurrentWord();
                    await _game.AnnounceScores();
                    await _game.TellTheWord(new HatWordToGuess {Value = (await _game.TakeWord())?.Value});
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