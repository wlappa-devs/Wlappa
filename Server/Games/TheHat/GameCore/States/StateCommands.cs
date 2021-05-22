using System;
using Server.Utils;

namespace Server.Games.TheHat.GameCore
{
    public interface IStateCommand
    {
        void Apply(HatGame game);
    }

    public class EndGame : IStateCommand
    {
        public void Apply(HatGame game)
        {
            game.EndGameEvent(new StubHatGameReport()); //TODO NormalReports
        }
    }

    public class MoveToNextPlayerPair : IStateCommand
    {
        public void Apply(HatGame game) =>
            game.CurrentPair = game.Mode switch
            {
                PairChoosingMode.Circle => GetNextPairCircle(game),
                PairChoosingMode.Pairs => GetNextPairPairs(game),
                _ => throw new ArgumentOutOfRangeException()
            };

        private (int, int) GetNextPairCircle(HatGame game)
        {
            var lapChanged = game.IncrementLapCountIfNeeded();
            var currentPair = game.CurrentPair;
            if (lapChanged != -1)
                currentPair = (currentPair.explainerIndex, (currentPair.understanderIndex + 1) % game.Players.Count);
            return currentPair.Select(x => (x + 1) % game.Players.Count);
        }

        private (int, int) GetNextPairPairs(HatGame game)
        {
            game.IncrementLapCountIfNeeded();
            var nextPair = game.CurrentPair.Select(x => (x + 2) % game.Players.Count);
            if (game.LapCount % 2 == 1)
                (nextPair.explainerIndex, nextPair.understanderIndex) =
                    (nextPair.understanderIndex, nextPair.explainerIndex);
            return nextPair;
        }
    }

    public class StartExplanation : IStateCommand
    {
        public void Apply(HatGame game)
        {
            game.Timer.RequestEventIn(game.TimeToExplain, new EndExplanation());
        }
    }

    public class EndExplanation : IStateCommand
    {
        public void Apply(HatGame game) { }
    }

    public class PickWord : IStateCommand
    {
        public void Apply(HatGame game)
        {
            game.TakeWord();
        }
    }

    public class GuessRight : IStateCommand
    {
        public void Apply(HatGame game)
        {
            game.Players[game.CurrentPair.explainerIndex].IncrementScore();
            game.Players[game.CurrentPair.understanderIndex].IncrementScore();
            game.AddGuessedWord();
        }
    }
}