using System;
using System.Threading.Tasks;
using Server.Games.TheHat.GameCore;
using Shared.Protos.HatSharedClasses;
using AddWords = Shared.Protos.HatSharedClasses.AddWords;

namespace Server.Games.TheHat.HatIGameStates
{
    public class AddingWordsState : IHatGameState
    {
        private readonly int _authorsReady;
        private readonly HatIGame _game;

        public AddingWordsState(int authorsReady, HatIGame game)
        {
            _authorsReady = authorsReady;
            _game = game;
        }

        public async Task<IHatGameState> HandleEvent(HatPlayer client, HatClientMessage e)
        {
            if (e is AddWords addWords)
            {
                var addingResult = await _game.AddWords(addWords.Value, client);
                if (_authorsReady + addingResult == _game.PlayersCount)
                {
                    await _game.SendMulticastMessage(new StartGame());
                    _game.CurrentPair = (0, 1);
                    await _game.AnnounceCurrentPair();
                    return new WaitingForPLayersToGetReady(false, false, _game);
                }
                return new AddingWordsState(_authorsReady + 1, _game);
            }

            throw new ArgumentOutOfRangeException(nameof(e), "Unexpected command");
        }
    }
}