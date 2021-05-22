using System;

namespace Server.Games.TheHat.GameCore.States
{
    public class StateInit : IState
    {
        public IState GetNextState(IStateCommand command, HatGame game) => command switch
        {
            MoveToNextPlayerPair moveToNextPlayerPair => new StatePairChosen(),
            EndGame endGame => new StateGameEnded(),
            _ => throw new ArgumentOutOfRangeException(nameof(command))
        };
    }
}