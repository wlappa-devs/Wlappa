using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.Views;
using Client_lib;
using Shared.Protos;
using Shared.Protos.HatSharedClasses;

namespace AndroidBlankApp1.ViewModels.GameViewModels
{
    public class HatViewModel
    {
        private readonly Game? _gameInstance;

        private Dictionary<Guid, int> _lastScores = new Dictionary<Guid, int>();
        private readonly IReadOnlyCollection<PlayerInLobby>? _players;

        public IReadOnlyDictionary<Guid, int> LastScores => _lastScores;

        public string LastScoresConcated =>
            string.Join("\n", LastScores.Select(x => $"{GetName(x.Key)} -- {x.Value}"));

        public Guid Explainer { get; set; }
        public Guid Understander { get; set; }

        public bool AmInPair => AmExplainer || AmUnderstander;

        public bool AmExplainer => _gameInstance!.PlayerId == Explainer;

        public bool AmUnderstander => _gameInstance!.PlayerId == Understander;

        private DateTime _timerStartMoment = DateTime.Now;
        public string TimerString => (DateTime.Now - _timerStartMoment).Seconds.ToString();

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
        public int AmountOfPlayersWithSelectedWords { get; private set; }

        public HatViewModel(GameInstanceProvider instanceProvider)
        {
            _gameInstance = instanceProvider.GameInstance;
            _players = instanceProvider.Players;
            _gameInstance!.MessageFromServer += HandleGameEvent;
        }

        public async void HandleGameEvent(InGameServerMessage message)
        {
            if (!(message is HatServerMessage hatServerMessage)) throw new ArgumentException();
            await HandleGameEvent(hatServerMessage);
        }

        public async Task HandleGameEvent(HatServerMessage message)
        {
            switch (message)
            {
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
                    GameOver?.Invoke();
                    break;
                case HatInvalidWordsSet hatInvalidWordsSet:
                    InvalidWordSet?.Invoke();
                    break;
                case HatPointsUpdated hatPointsUpdated:
                    _lastScores = hatPointsUpdated.GuidToPoints;
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

                    AmountOfPlayersWithSelectedWords = playerSuccessfullyAddedWords.TotalReady;
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