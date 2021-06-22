﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.Util; // TODO replace android logging with generic logging
using Client_lib;
using Shared.Protos;
using Shared.Protos.HatSharedClasses;

namespace AndroidClient.ViewModels.GameViewModels
{
    public class HatViewModel : IGameViewModel
    {
        private readonly Game _gameInstance;
        private Dictionary<Guid, int> _lastScores = new Dictionary<Guid, int>();
        private readonly IReadOnlyCollection<PlayerInLobby> _players;

        // Needed for future better score display
        // ReSharper disable once MemberCanBePrivate.Global
        public IReadOnlyDictionary<Guid, int> LastScores => _lastScores;

        public List<(string Name, int Value)> LastScoresValues =>
            LastScores.Select(pair => (GetName(pair.Key), pair.Value)).ToList();

        public Guid Explainer { get; private set; }
        public Guid Understander { get; private set; }

        private Guid? _manager;
        private int _wordsToWrite;

        public string? MyRole { get; private set; }
        private bool IsManaged => _manager.HasValue;

        private bool AmManager => _gameInstance!.PlayerId == _manager;

        public bool AmInPair => AmExplainer || AmUnderstander;

        private bool AmExplainer => _gameInstance!.PlayerId == Explainer;

        private bool AmUnderstander => _gameInstance!.PlayerId == Understander;

        public bool AmSpectator => MyRole == HatRoleSpectator.Value;

        public bool AmControllingExplanation => IsManaged ? AmManager : AmExplainer;

        public bool AmSeeingWord => AmExplainer || AmManager || AmSpectator;

        private DateTime _timerStartMoment = DateTime.Now;
        private TimeSpan _timeToExplain = TimeSpan.Zero;

        public string TimerString
        {
            get
            {
                var output = (_timeToExplain - (DateTime.Now - _timerStartMoment)).TotalSeconds;
                return output > 0 ? $"{output:00.00}" : "00.00";
            }
        }

        public string[]? WordsInput { get; private set; }
        public string? CurrentWord { get; private set; }

        public event Action? WordsSuccessfullyAddedByMe;
        public event Action? WordsSuccessfullyAddedBySomeOne;
        public event Action? AnnouncedNextPair;
        public event Action? ScoresUpdated;
        public event Action<IReadOnlyCollection<int>>? InvalidWordSet;
        public event Action? StartExplanation;
        public event Action? GetWord;
        public event Action? TimeIsUp;
        public event Action? GameOver;
        public event Action? GotGameInitialMessage;

        public int RemainingPlayersToWriteWords { get; private set; }

        public HatViewModel(Game gameInstance, IReadOnlyCollection<PlayerInLobby> players)
        {
            _gameInstance = gameInstance;
            _players = players;
            _gameInstance!.MessageFromServer = HandleGameEvent;
        }

        private void HandleGameEvent(InGameServerMessage message)
        {
            if (!(message is HatServerMessage hatServerMessage)) throw new ArgumentException();
            HandleGameEvent(hatServerMessage);
        }

        public async Task CancelExplanation()
        {
            if (AmControllingExplanation)
            {
                await _gameInstance!.SendGameEvent(new HatCancelExplanation());
            }
        }

        private void HandleGameEvent(HatServerMessage message)
        {
            switch (message)
            {
                case HatGameInitialInformationMessage informationMessage:
                    _manager = informationMessage.ManagerId;
                    MyRole = informationMessage.Role;
                    RemainingPlayersToWriteWords = informationMessage.NumberOfPlayersInGame;
                    _timeToExplain = informationMessage.TimeToExplain;
                    _wordsToWrite = informationMessage.WordsToWrite;
                    WordsInput = Enumerable.Repeat(string.Empty, _wordsToWrite).ToArray();
                    Log.Info(nameof(HatViewModel), $"My role is {MyRole}");
                    Log.Info(nameof(HatViewModel), $"I need to write {_wordsToWrite} words");
                    if (IsManaged)
                        Log.Info(nameof(HatViewModel), $"Manager exists, and his GUID is {_manager!.Value}");
                    WordsSuccessfullyAddedBySomeOne?.Invoke();
                    GotGameInitialMessage?.Invoke();
                    break;
                case HatAnnounceNextPair hatAnnounceNextPair:
                    Explainer = hatAnnounceNextPair.Explainer;
                    Understander = hatAnnounceNextPair.Understander;
                    AnnouncedNextPair?.Invoke();
                    break;
                case HatExplanationStarted _:
                    CurrentWord = "";
                    _timerStartMoment = DateTime.Now;
                    StartExplanation?.Invoke();
                    break;
                case HatFinishMessage _:
                    Log.Info(nameof(HatViewModel), "Should finish game");
                    GameOver?.Invoke();
                    break;
                case HatInvalidWordsSet hatInvalidWordsSet:
                    InvalidWordSet?.Invoke(hatInvalidWordsSet.WrongWordIds);
                    break;
                case HatPointsUpdated hatPointsUpdated:
                    _lastScores = hatPointsUpdated.GuidToPoints;
                    Log.Info(nameof(HatViewModel), "Scores update");
                    ScoresUpdated?.Invoke();
                    break;
                case HatStartGame _:
                    break;
                case HatTimeIsUp _:
                    TimeIsUp?.Invoke();
                    break;
                case HatWordToGuess hatWordToGuess:
                    CurrentWord = hatWordToGuess.Value;
                    GetWord?.Invoke();
                    break;
                case HatPlayerSuccessfullyAddedWords playerSuccessfullyAddedWords:
                    if (playerSuccessfullyAddedWords.AuthorId == _gameInstance!.PlayerId)
                        WordsSuccessfullyAddedByMe?.Invoke();

                    RemainingPlayersToWriteWords = playerSuccessfullyAddedWords.TotalNotReady;
                    WordsSuccessfullyAddedBySomeOne?.Invoke();
                    return;
                default:
                    throw new ArgumentOutOfRangeException(nameof(message));
            }
        }

        public async Task SendWords()
        {
            await _gameInstance!.SendGameEvent(new HatAddWords()
            {
                Value = WordsInput!.ToList()
            });
        }

        public async Task GetReady()
        {
            await _gameInstance!.SendGameEvent(new HatClientIsReady());
        }

        public async Task GuessWord()
        {
            await _gameInstance!.SendGameEvent(new HatGuessRight());
        }

        public string GetName(Guid guid) => _players!.First(x => x.Id == guid).Name;
    }
}