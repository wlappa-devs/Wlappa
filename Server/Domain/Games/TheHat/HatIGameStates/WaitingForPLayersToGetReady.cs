using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Shared.Protos.HatSharedClasses;

namespace Server.Domain.Games.TheHat.HatIGameStates
{
    public class WaitingForPLayersToGetReady : IHatGameState
    {
        private readonly bool _explainerIsReady;
        private readonly bool _understanderIsReady;
        private readonly HatGame _game;

        public WaitingForPLayersToGetReady(bool explainerIsReady, bool understanderIsReady, HatGame game)
        {
            _explainerIsReady = explainerIsReady;
            _understanderIsReady = understanderIsReady;
            _game = game;
        }

        public async Task<IHatGameState> HandleEvent(HatMember client, HatClientMessage e)
        {
            if (client is not HatPlayer player) throw new ArgumentOutOfRangeException(nameof(e), "Unexpected command");
            if (e is not HatClientIsReady) throw new ArgumentOutOfRangeException(nameof(e), "Unexpected command");

            if (player.Id == _game.CurrentPair.explainerIndex)
            {
                if (_understanderIsReady) return await BothAreReady();
                _game.Logger.Log(LogLevel.Information, "explainer is ready");
                return new WaitingForPLayersToGetReady(true, false, _game);
            }

            if (player.Id != _game.CurrentPair.understanderIndex)
                throw new ArgumentOutOfRangeException(nameof(e), "Unexpected command");
            if (_explainerIsReady) return await BothAreReady();
            _game.Logger.Log(LogLevel.Information, "understander is ready");
            return new WaitingForPLayersToGetReady(false, true, _game);
        }

        private async Task<IHatGameState> BothAreReady()
        {
            _game.Logger.Log(LogLevel.Information, "both are ready");
            _game.SetTimerForExplanation();
            await _game.AnnounceScores();
            await _game.TellTheWord(new HatWordToGuess {Value = (await _game.TakeWord())?.Value});
            return new ExplanationInProcess(_game);
        }
    }
}