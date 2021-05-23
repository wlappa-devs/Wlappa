using System;
using System.Text.RegularExpressions;

namespace Server.Games.TheHat.GameCore.States
{
    public class StatePairChosen : IState
    {
        public IState GetNextState(IStateCommand command, HatGame game) => command switch
        {
            AddWords addWord => this,
            EndGame endGame => new StateGameEnded(),
            StartExplanation startExplanation => new StateExplanationStarted(),
            _ => throw new ArgumentOutOfRangeException(nameof(command))
        };
    }
}