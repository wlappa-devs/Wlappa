using System;

namespace Server.Games.TheHat.GameCore.States
{
    public class StateWordPicked : IState
    {
        public IState GetNextState(IStateCommand command, HatGame game)
        {
            if (game.WordsRemaining > 0)
            {
                return command switch
                {
                    GuessRight guessRight => new StateExplanationStarted(),
                    EndExplanation endExplanation => new StateInit(),
                    _ => throw new ArgumentOutOfRangeException(nameof(command))
                };
            }
            return command switch
            {
                GuessRight guessRight => new StateGameEnded(),
                EndExplanation endExplanation => new StateInit(),
                _ => throw new ArgumentOutOfRangeException(nameof(command))
            };
        }
    }
}