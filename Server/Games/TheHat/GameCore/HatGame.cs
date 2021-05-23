using System;
using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.WellKnownTypes;
using Server.Games.Meta;
using Server.Games.TheHat.GameCore.States;
using Server.Games.TheHat.GameCore.Timer;
using Server.Utils;
using Shared.Protos.HatSharedClasses;

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
        public int WordsRemaining => _words.Count;
        public Word? CurrentWord { get; private set; }
        private readonly List<Word> _guessedWords;
        public IReadOnlyList<Word> GuessedWords => _guessedWords;
        public (int explainerIndex, int understanderIndex) CurrentPair { get; set; }
        public IState CurrentState { get; private set; }
        private Random _random;

        public HatGame(IEnumerable<HatPlayer> players, int wordsToBeWritten, PairChoosingMode mode,  
            Random random, ITimer timer, Duration timeToExplain, Action<IHatGameReport> endGameEvent)
        {
            Mode = mode;
            Timer = timer;
            TimeToExplain = timeToExplain;
            EndGameEvent = endGameEvent;
            _random = random;
            CurrentPair = (0, 0);
            CurrentState = new StateWritingWords();
            _words = new List<Word>();
            _guessedWords = new List<Word>();
            Players = players.OrderBy(x => x.Id).ToList();
        }

        public void AddWord(Word word)
        {
            _words.Add(word);
        }

        public void TreatCommand(IStateCommand command)
        {
            CurrentState = CurrentState.ChangeState(command, this);
        }

        public Word? TakeWord()
        {
            if (WordsRemaining==0)
                return CurrentWord;
            
            var randomIndex = _random.Next(0, _words.Count);
            (_words[^1], _words[randomIndex]) = (_words[randomIndex], _words[^1]);
            _words[^1].AddGuessTry();
            return CurrentWord = _words.RemoveAtAndReturn(_words.Count - 1);
        }

        public void ReturnWord()
        {
            if (CurrentWord is null) return;
            _words.Add(CurrentWord);
            CurrentWord = null;
        }

        private int IncrementLapCount() => ++LapCount;
        
        public int IncrementLapCountIfNeeded() =>
            CurrentPair.explainerIndex == Players.Count - 1
                ? IncrementLapCount()
                : -1;

        public void AddGuessedWord() {
            _guessedWords.Add(CurrentWord!);
            CurrentWord = null;
        }
    }
}