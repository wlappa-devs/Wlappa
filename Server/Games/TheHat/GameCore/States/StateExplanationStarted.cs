using System;

namespace Server.Games.TheHat.GameCore.States
{
    public class StateExplanationStarted : IState
    {
        public IState GetNextState(IStateCommand command, HatGame game) => command switch
        {
            EndExplanation endExplanation => new StateInit(),
            PickWord pickWord => new StateWordPicked(),
            _ => throw new ArgumentOutOfRangeException(nameof(command))
        };
    }
}