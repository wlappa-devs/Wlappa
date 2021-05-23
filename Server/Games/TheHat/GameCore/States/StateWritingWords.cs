using System;

namespace Server.Games.TheHat.GameCore.States
{
    public class StateWritingWords : IState
    {
        private readonly int _authorsFinished;

        public StateWritingWords(int authorsFinished = 0)
        {
            _authorsFinished = authorsFinished;
        }

        public IState GetNextState(IStateCommand command, HatGame game) => command switch
        {
            AddWords addWords => _authorsFinished + 1 == game.Players.Count
                ? new StateInit()
                : new StateWritingWords(_authorsFinished + 1),
            _ => throw new ArgumentOutOfRangeException(nameof(command))
        };
    }
}