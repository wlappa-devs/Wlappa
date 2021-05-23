using System;
using System.Collections.Generic;

namespace Server.Games.TheHat.GameCore
{
    public interface IState
    {
        public IState ChangeState(IStateCommand command, HatGame game)
        {
            var nextState = GetNextState(command, game);
            command.Apply(game);
            return nextState;
        }

        IState GetNextState(IStateCommand command, HatGame game);
    }
}