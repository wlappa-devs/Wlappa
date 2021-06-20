using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Server.Domain.Games.Meta;
using Server.Domain.Games.TheHat.HatIGameStates;
using Server.Utils;
using Shared.Protos;
using Shared.Protos.HatSharedClasses;

namespace Server.Domain.Games.TheHat
{
    public class HatGame : Game
    {
        // TODO Check hat words availability at the timer event
        private readonly Func<Task> _finished;
        private readonly ITimer _timer;
        private readonly Random _random;
        private bool _isFinished;
        public ILogger<HatGame> Logger { get; }
        private readonly int _wordsToBeWritten;
        private readonly HatGameModeConfiguration _mode;
        private readonly TimeSpan _timeToExplain;

        private IHatGameState _currentState;
        private readonly Dictionary<Guid, HatMember> _clientToMemberMapping;
        private readonly List<Word> _words;

        private readonly HatMember? _manger;
        private readonly List<HatMember> _allMembers = new();
        private readonly List<HatPlayer> _players = new();

        private readonly MulticastGroup _membersWhoNeedsToKnowTheWordExceptExplainer;
        private readonly MulticastGroup _allMembersGroup;

        private HatPlayer Understander => _players[CurrentPair.understanderIndex];
        private HatPlayer Explainer => _players[CurrentPair.explainerIndex];

        public HatMember ExplanationControllingMember => _manger is not null ? _manger! : Explainer;
        private Word? CurrentWord { get; set; }
        public int WordsRemaining => _words.Count;
        public (int explainerIndex, int understanderIndex) CurrentPair { get; set; }
        public int PlayersCount => _players.Count;
        public int LapCount { get; private set; } = 1;

        private Guid? _currentExplanationId;

        public HatGame(HatConfiguration configuration, GameCreationPayload payload, Func<Task> finished, ITimer timer,
            Random random,
            ILogger<HatGame> logger)
        {
            _finished = finished;
            _timer = timer;
            _random = random;
            Logger = logger;
            _wordsToBeWritten = configuration.WordsToBeWritten;
            _mode = configuration.HatGameModeConfiguration;
            _timeToExplain = configuration.TimeToExplain;

            #region PayLoad Deconstruction

            var players = payload.Clients;
            var index = 0;
            var listOfPlayersWhoNeedToKnowTheWordExceptExplainer = new List<HatMember>();
            foreach (var player in players)
            {
                var playerRole = IHatRole.GetRoleByString(payload.PlayerToRole[player.Id]);
                if (playerRole is HatRolePlayer hatRolePlayer)
                {
                    var member = new HatPlayer(player, index, hatRolePlayer);
                    index++;
                    _players.Add(member);
                    _allMembers.Add(member);
                }
                else if (playerRole is HatRoleObserver hatRoleObserver)
                {
                    var member = new HatMember(player, hatRoleObserver);
                    _allMembers.Add(member);
                }
                else if (playerRole is HatRoleKukold hatRoleKukold)
                {
                    var member = new HatMember(player, hatRoleKukold);
                    _allMembers.Add(member);
                    listOfPlayersWhoNeedToKnowTheWordExceptExplainer.Add(member);
                }
                else if (playerRole is HatRoleManager hatRoleManager)
                {
                    logger.Log(LogLevel.Information, $"Manager appeared in payload {player.Id}");
                    var member = new HatMember(player, hatRoleManager);
                    _manger = member;
                    _allMembers.Add(member);
                    listOfPlayersWhoNeedToKnowTheWordExceptExplainer.Add(member);
                }
            }

            #endregion

            _allMembersGroup = new MulticastGroup(players);
            _membersWhoNeedsToKnowTheWordExceptExplainer =
                new MulticastGroup(listOfPlayersWhoNeedToKnowTheWordExceptExplainer.Select(e => e.ClientInteractor)
                    .ToArray());

            _words = new List<Word>();
            _clientToMemberMapping = _allMembers
                .ToDictionary(player => player.ClientInteractor.Id, player => player);
            _currentState = new AddingWordsState(0, this);
        }

        public void CancelTimer()
        {
            if (!_currentExplanationId.HasValue) return;
            _timer.CancelEvent(_currentExplanationId.Value);
            _currentExplanationId = null;
        }

        protected override async Task UnsafeHandleEvent(Guid? clientId, InGameClientMessage e)
        {
            if (e is HatClientMessage hatClientMessage)
                if (clientId is null)
#pragma warning disable 8625
                    _currentState = await _currentState.HandleEvent(null, hatClientMessage);
#pragma warning restore 8625

                else
                    _currentState =
                        await _currentState.HandleEvent(_clientToMemberMapping[clientId.Value], hatClientMessage);
            else
                throw new ArgumentException("Wrong game, dude");
        }

        protected override async Task UnsafeInitialize()
        {
            foreach (var member in _allMembers)
                await member.HandleServerMessage(
                    new HatGameInitialInformationMessage
                    {
                        ManagerId = _manger?.ClientInteractor.Id,
                        Role = member.Role.StringValue,
                        NumberOfPlayersInGame = _players.Count,
                        TimeToExplain = _timeToExplain,
                        WordsToWrite = _wordsToBeWritten
                    }
                );
        }

        private List<int> ValidateWords(IReadOnlyList<string> words)
        {
            var filteredWords = words
                .Select((word, index) => (word,index))
                .Where(pair => pair.word == "")
                .Select(pair => pair.index).ToList();
            return filteredWords;
        }

        public async Task<int> AddWords(IReadOnlyList<string> words, HatPlayer author)
        {
            var validationResult = ValidateWords(words);
            if (validationResult.Count == 0)
            {
                _words.AddRange(words.Select(x => new Word(x, author)));
                return 1;
            }
            await author.HandleServerMessage(new HatInvalidWordsSet{WrongWordIds = validationResult});
            return 0;
        }

        public Task SendMulticastMessage(HatServerMessage message) =>
            _allMembersGroup.SendMulticastEvent(message);

        public async Task AnnounceCurrentPair()
        {
            if (_isFinished) return;
            Logger.LogInformation($"{Explainer.ClientInteractor.Name} => {Understander.ClientInteractor.Name}");
            await SendMulticastMessage(new HatAnnounceNextPair
            {
                Explainer = Explainer.ClientInteractor.Id,
                Understander = Understander.ClientInteractor.Id
            });
        }

        public Task AnnounceScores() =>
            SendMulticastMessage(new HatPointsUpdated {GuidToPoints = GenerateGuidToPoints()});

        public void SetTimerForExplanation()
        {
            _currentExplanationId = Guid.NewGuid();
            _timer.RequestEventIn(_timeToExplain, new HatTimerFinish(), _currentExplanationId.Value);
            SendMulticastMessage(new HatExplanationStarted());
        }

        public async Task<Word?> TakeWord()
        {
            if (_isFinished) return CurrentWord;
            if (WordsRemaining == 0)
            {
                await FinishGame(new HatNoWordsLeft());
                return CurrentWord;
            }

            var randomIndex = _random.Next(0, _words.Count);
            (_words[^1], _words[randomIndex]) = (_words[randomIndex], _words[^1]);
            _words[^1].AddGuessTry();
            return CurrentWord = _words.RemoveAtAndReturn(_words.Count - 1);
        }

        public async Task TellTheWord(HatServerMessage message)
        {
            await Explainer.HandleServerMessage(message);
            await _membersWhoNeedsToKnowTheWordExceptExplainer.SendMulticastEvent(message);
        }

        public void GuessCurrentWord()
        {
            Explainer.IncrementScore();
            Understander.IncrementScore();
            CurrentWord = null;
        }

        public async void MoveToNextPair()
        {
            if (_isFinished) return;
            Logger.LogInformation(
                $"LapCount: {LapCount}, Explainer: {CurrentPair.explainerIndex}, PlayersCount: {PlayersCount}");
            if (_mode.GameIsOver(LapCount, CurrentPair.explainerIndex, PlayersCount))
            {
                await FinishGame(new HatRotationFinished());
                return;
            }

            if (_mode is HatCircleChoosingModeConfiguration)
                CurrentPair = GetNextPairCircle();
            if (_mode is HatPairChoosingModeConfiguration)
                CurrentPair = GetNextPairPairs();
        }

        private async Task FinishGame(HatFinishMessage message)
        {
            _isFinished = true;
            Logger.LogInformation("Game should be ended");
            CancelTimer();
            await SendMulticastMessage(message);
            await _finished();
        }

        private (int, int) GetNextPairCircle()
        {
            var lapChanged = IncrementLapCountIfNeeded();
            var currentPair = CurrentPair;
            if (lapChanged != -1)
                currentPair = (currentPair.explainerIndex, (currentPair.understanderIndex + 1) % PlayersCount);
            return currentPair.Select(x => (x + 1) % PlayersCount);
        }

        private (int, int) GetNextPairPairs()
        {
            IncrementLapCountIfNeeded();
            var nextPair = CurrentPair.Select(x => (x + 2) % PlayersCount);
            if (LapCount % 2 == 1)
                (nextPair.explainerIndex, nextPair.understanderIndex) =
                    (nextPair.understanderIndex, nextPair.explainerIndex);
            return nextPair;
        }

        private int IncrementLapCountIfNeeded() =>
            CurrentPair.explainerIndex == PlayersCount - 1
                ? ++LapCount
                : -1;

        public void ReturnCurrentWordInHatIfNeeded()
        {
            if (CurrentWord is null) return;
            _words.Add(CurrentWord);
            CurrentWord = null;
        }

        private Dictionary<Guid, int> GenerateGuidToPoints() =>
            _players.ToDictionary(
                x => x.ClientInteractor.Id,
                x => x.Score
            );
    }
}