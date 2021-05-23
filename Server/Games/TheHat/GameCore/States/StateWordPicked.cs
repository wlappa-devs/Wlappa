using System;

namespace Server.Games.TheHat.GameCore.States
{
    public class StateWordPicked : IState
    {
        public IState GetNextState(IStateCommand command, HatGame game)
        {
            return command switch
            {
                GuessRight guessRight =>
                    game.WordsRemaining > 0 ? new StateExplanationStarted() : new StateGameEnded(),
                EndExplanation endExplanation => new StateInit(),
                _ => throw new ArgumentOutOfRangeException(nameof(command))
            };
        }
    }
}