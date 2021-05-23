namespace Server.Games.TheHat.GameCore.States
{
    public class StateGameEnded : IState
    {
        public IState GetNextState(IStateCommand command, HatGame game) => this;
    }
}