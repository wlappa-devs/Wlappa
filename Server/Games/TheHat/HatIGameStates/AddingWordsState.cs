using System;
using System.Threading.Tasks;
using Server.Games.TheHat.GameCore;
using Shared.Protos.HatSharedClasses;

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

        public async Task<IHatGameState> HandleEvent(HatMember client, HatClientMessage e)
        {
            if (client is HatPlayer player)
            {
                if (e is HatAddWords addWords)
                {
                    var addingResult = await _game.AddWords(addWords.Value, player);
                    if (addingResult != 1) return this;
                    await _game.SendMulticastMessage(new HatPlayerSuccessfullyAddedWords
                    {
                        AuthorId = client.Client.Id,
                        TotalNotReady = _game.PlayersCount - _authorsReady - 1
                    });
                    if (_authorsReady + 1 == _game.PlayersCount)
                    {
                        await _game.SendMulticastMessage(new HatStartGame());
                        _game.CurrentPair = (0, 1);
                        
                        await _game.AnnounceScores();
                        
                        await _game.AnnounceCurrentPair();
                        
                        
                        return new WaitingForPLayersToGetReady(false, false, _game);
                    }

                    return new AddingWordsState(_authorsReady + 1, _game);
                }
            }
            else return this;

            throw new ArgumentOutOfRangeException(nameof(e), "Unexpected command");
        }
    }


}