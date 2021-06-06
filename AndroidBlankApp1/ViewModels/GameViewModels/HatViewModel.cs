using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.Util;
using Android.Views;
using AndroidBlankApp1.ViewModels.Providers;
using Client_lib;
using Shared.Protos;
using Shared.Protos.HatSharedClasses;

namespace AndroidBlankApp1.ViewModels.GameViewModels
{
    public class HatViewModel : IGameViewModel
    {
        private readonly Game _gameInstance;

        private Dictionary<Guid, int> _lastScores = new Dictionary<Guid, int>();
        private readonly IReadOnlyCollection<PlayerInLobby> _players;

        public IReadOnlyDictionary<Guid, int> LastScores => _lastScores;

        public string LastScoresConcated =>
            string.Join("\n", LastScores.Select(x => $"{GetName(x.Key)} -- {x.Value}"));

        public Guid Explainer { get; set; }
        public Guid Understander { get; set; }

        public Guid? Manager { get; set; }

        public string MyRole { get; set; }
        public bool IsManaged => Manager.HasValue;

        public bool AmManager => _gameInstance!.PlayerId == Manager;

        public bool AmInPair => AmExplainer || AmUnderstander;

        public bool AmExplainer => _gameInstance!.PlayerId == Explainer;

        public bool AmUnderstander => _gameInstance!.PlayerId == Understander;

        public bool AmControllingExplanation => IsManaged ? AmManager : AmExplainer;

        private DateTime _timerStartMoment = DateTime.Now;
        private TimeSpan _timeToExplain = TimeSpan.Zero;
        public string TimerString => (_timeToExplain - (DateTime.Now - _timerStartMoment)).Seconds.ToString();

        public string CurrentWord { get; set; }

        public event Action WordsSuccessfullyAddedByMe;
        public event Action WordsSuccessfullyAddedBySomeOne;
        public event Action AnnouncedNextPair;
        public event Action ScoresUpdated;
        public event Action InvalidWordSet;
        public event Action StartExplanation;
        public event Action GetWord;
        public event Action TimeIsUp;
        public event Action GameOver;

        public string WordsInput { get; set; }
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
                    Manager = informationMessage.ManagerId;
                    MyRole = informationMessage.Role;
                    RemainingPlayersToWriteWords = informationMessage.NumberOfPlayersInGame;
                    _timeToExplain = informationMessage.TimeToExplain;
                    Log.Info(nameof(HatViewModel), $"My role is {MyRole}");
                    if (IsManaged)
                        // ReSharper disable once PossibleInvalidOperationException
                        Log.Info(nameof(HatViewModel), $"Manager exists, and his GUID is {Manager.Value}");
                    WordsSuccessfullyAddedBySomeOne?.Invoke();
                    break;
                case HatAnnounceNextPair hatAnnounceNextPair:
                    Explainer = hatAnnounceNextPair.Explainer;
                    Understander = hatAnnounceNextPair.Understander;
                    AnnouncedNextPair?.Invoke();
                    break;
                case HatExplanationStarted hatExplanationStarted:
                    CurrentWord = "";
                    _timerStartMoment = DateTime.Now;
                    StartExplanation?.Invoke();
                    break;
                // case HatNoWordsLeft hatNoWordsLeft:
                // break;
                // case HatRotationFinished hatRotationFinished:
                // break;
                case HatFinishMessage hatFinishMessage:
                    Log.Info(nameof(HatViewModel), "Should finish game");
                    GameOver?.Invoke();
                    break;
                case HatInvalidWordsSet hatInvalidWordsSet:
                    InvalidWordSet?.Invoke();
                    break;
                case HatPointsUpdated hatPointsUpdated:
                    _lastScores = hatPointsUpdated.GuidToPoints;
                    Log.Info(nameof(HatViewModel), "Scores update");
                    ScoresUpdated?.Invoke();
                    break;
                case HatStartGame hatStartGame:
                    break;
                case HatTimeIsUp hatTimeIsUp:
                    TimeIsUp?.Invoke();
                    break;
                case HatWordToGuess hatWordToGuess:
                    CurrentWord = hatWordToGuess.Value;
                    GetWord?.Invoke();
                    break;
                case HatPlayerSuccessfullyAddedWords playerSuccessfullyAddedWords:
                    if (playerSuccessfullyAddedWords.AuthorId == _gameInstance!.PlayerId)
                    {
                        WordsSuccessfullyAddedByMe?.Invoke();
                    }

                    RemainingPlayersToWriteWords = playerSuccessfullyAddedWords.TotalNotReady;
                    WordsSuccessfullyAddedBySomeOne?.Invoke();
                    return;
                default:
                    throw new ArgumentOutOfRangeException(nameof(message));
            }
        }

        public async Task SendWords(View view)
        {
            await _gameInstance!.SendGameEvent(new HatAddWords()
            {
                Value = WordsInput.Split(" ")
            });
        }

        public async Task GetReady(object sender, EventArgs args)
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