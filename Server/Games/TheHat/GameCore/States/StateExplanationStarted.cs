using System;

namespace Server.Games.TheHat.GameCore.States
{
    public class StateExplanationStarted : IState
    {
        public IState GetNextState(IStateCommand command, HatGame game) => command switch
        {
            EndExplanation endExplanation => new StateInit(),
            PickWord pickWord => game.WordsRemaining > 0 ? new StateWordPicked() : throw new InvalidOperationException("The hat is empty."),
            _ => throw new ArgumentOutOfRangeException(nameof(command))
        };
    }
}