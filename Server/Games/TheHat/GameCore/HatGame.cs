using System;
using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.WellKnownTypes;
using Server.Games.Meta;
using Server.Games.TheHat.GameCore.States;
using Server.Games.TheHat.GameCore.Timer;
using Server.Utils;

namespace Server.Games.TheHat.GameCore
{
    public class HatGame
    {
        public PairChoosingMode Mode { get; }
        public ITimer Timer { get; }
        public Duration TimeToExplain { get; }
        public Action<IHatGameReport> EndGameEvent { get; }
        public List<HatPlayer> Players { get; }
        
        
        public int LapCount { get; private set; }

        private readonly List<Word> _words;
        public Word CurrentWord { get; private set; }
        private readonly List<Word> _guessedWords;
        public IReadOnlyList<Word> GuessedWords => _guessedWords;
        public (int explainerIndex, int understanderIndex) CurrentPair { get; set; }
        public IState CurrentState { get; private set; }
        private Random _random;

        public HatGame(IEnumerable<HatPlayer> players, IEnumerable<Word> words, PairChoosingMode mode,  
            Random random, ITimer timer, Duration timeToExplain, Action<IHatGameReport> endGameEvent)
        {
            Mode = mode;
            Timer = timer;
            TimeToExplain = timeToExplain;
            EndGameEvent = endGameEvent;
            _random = random;
            CurrentState = new StatePairChosen();
            CurrentPair = (0, 1);
            _words = words.ToList();
            _guessedWords = new List<Word>();
            Players = players.OrderBy(x => x.Id).ToList();
        }

        public void TreatCommand(IStateCommand command)
        {
            CurrentState = CurrentState.ChangeState(command, this);
        }

        public Word TakeWord()
        {
            var randomIndex = _random.Next(0, _words.Count);
            (_words[^1], _words[randomIndex]) = (_words[randomIndex], _words[^1]);
            _words[^1].AddGuessTry();
            return CurrentWord = _words.RemoveAtAndReturn(_words.Count - 1);
            
        }

        public void AddWord(Word word) => _words.Add(word);

        private int IncrementLapCount() => ++LapCount;
        
        public int IncrementLapCountIfNeeded() =>
            CurrentPair.explainerIndex == Players.Count - 1
                ? IncrementLapCount()
                : -1;

        public void AddGuessedWord() => _guessedWords.Add(CurrentWord);
    }
}