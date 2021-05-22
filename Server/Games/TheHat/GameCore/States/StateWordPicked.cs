using System;

namespace Server.Games.TheHat.GameCore.States
{
    public class StateWordPicked : IState
    {
        public IState GetNextState(IStateCommand command, HatGame game) => command switch
        {
            GuessRight guessRight => new StateExplanationStarted(),
            _ => throw new ArgumentOutOfRangeException(nameof(command))
        };
    }
}