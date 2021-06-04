using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;
using Server.Games.Meta;
using Server.Games.TheHat.GameCore;
using Server.Games.TheHat.HatIGameStates;
using Server.Routing;
using Server.Routing.Helpers;
using Server.Utils;
using Shared.Protos;
using Shared.Protos.HatSharedClasses;

namespace Server.Games.TheHat
{
    public class HatIGame : Game
    {
        // TODO Check hat words availability at the timer event
        private readonly Func<Task> _finished;
        private readonly ITimer _timer;
        private readonly Random _random;
        private bool _isFinished;
        public ILogger<HatIGame> Logger { get; }
        private readonly int _wordsToBeWritten;
        private readonly HatGameModeConfiguration _mode;
        private readonly TimeSpan _timeToExplain;

        private IHatGameState _currentState;
        private readonly Dictionary<IInGameClient, HatMember> _clientToMemberMapping;
        private readonly List<Word> _words;

        public bool IsManged => Manger is not null;
        public HatMember? Manger;
        private readonly List<HatMember> _allMembers = new();
        private readonly List<HatPlayer> _players = new();
        private readonly List<HatMember> _observers = new();
        private readonly List<HatMember> _kukolds = new();

        private MulticastGroup _membersWhoNeedsToKnowTheWordExceptExplainer;
        private readonly MulticastGroup _allMembersGroup;

        public HatPlayer Understander => _players[CurrentPair.understanderIndex];
        public HatPlayer Explainer => _players[CurrentPair.explainerIndex];
        public Word? CurrentWord { get; private set; }
        public int WordsRemaining => _words.Count;
        public (int explainerIndex, int understanderIndex) CurrentPair { get; set; }
        public int PlayersCount => _players.Count;
        public int LapCount { get; private set; } = 1;

        private Guid? _currentExplanationId;

        public HatIGame(HatConfiguration configuration, GameCreationPayload payload,
            IReadOnlyCollection<IInGameClient> players, Func<Task> finished, ITimer timer, Random random,
            ILogger<HatIGame> logger)
        {
            _finished = finished;
            _timer = timer;
            _random = random;
            Logger = logger;
            _wordsToBeWritten = configuration.WordsToBeWritten;
            _mode = configuration.HatGameModeConfiguration;
            _timeToExplain = configuration.TimeToExplain;

            #region PayLoad Deconstruction

            var index = 0;
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
                    _observers.Add(member);
                    _allMembers.Add(member);
                }
                else if (playerRole is HatRoleKukold hatRoleKukold)
                {
                    var member = new HatMember(player, hatRoleKukold);
                    _kukolds.Add(member);
                    _allMembers.Add(member);
                }
                else if (playerRole is HatRoleManager hatRoleManager)
                {
                    logger.Log(LogLevel.Information, $"Manager appeared in payload {player.Id}");
                    var member = new HatMember(player, hatRoleManager);
                    Manger = member;
                    _allMembers.Add(member);
                }
            }

            #endregion

            _allMembersGroup = new MulticastGroup(players);
            _membersWhoNeedsToKnowTheWordExceptExplainer = new MulticastGroup(_kukolds.Select(x => x.Client).ToList());

            _words = new List<Word>();
            _clientToMemberMapping = _allMembers
                .ToDictionary(player => player.Client, player => player);
            _currentState = new AddingWordsState(0, this);
        }

        public void CancelTimer()
        {
            if (!_currentExplanationId.HasValue) return;
            _timer.CancelEvent(_currentExplanationId.Value);
            _currentExplanationId = null;
        }

        protected override async Task UnsafeHandleEvent(IInGameClient? client, InGameClientMessage e)
        {
            if (e is HatClientMessage hatClientMessage)
                if (client is null)
#pragma warning disable 8625
                    _currentState = await _currentState.HandleEvent(null, hatClientMessage);
#pragma warning restore 8625

                else
                    _currentState = await _currentState.HandleEvent(_clientToMemberMapping[client], hatClientMessage);
            else
                throw new Exception("Wrong game, dude");
        }

        protected override async Task UnsafeInitialize()
        {
            foreach (var member in _allMembers)
                await member.HandleServerMessage(
                    new HatGameInitialInformationMessage
                    {
                        ManagerId = Manger?.Client.Id,
                        Role = member.Role.StringValue,
                        NumberOfPlayersInGame = _players.Count
                    }
                );
        }

        private bool ValidateWords(IReadOnlyList<string> words) => words.Count == _wordsToBeWritten;

        public async Task<int> AddWords(IReadOnlyList<string> words, HatPlayer author)
        {
            if (ValidateWords(words))
            {
                _words.AddRange(words.Select(x => new Word(x, author)));
                return 1;
            }

            await author.HandleServerMessage(new HatInvalidWordsSet());
            return 0;
        }

        public Task SendMulticastMessage(HatServerMessage message) =>
            _allMembersGroup.SendMulticastEvent(message);

        public async Task AnnounceCurrentPair()
        {
            if (_isFinished) return;
            Logger.LogInformation($"{Explainer.Client.Name} => {Understander.Client.Name}");
            await SendMulticastMessage(new HatAnnounceNextPair
            {
                Explainer = Explainer.Client.Id,
                Understander = Understander.Client.Id
            });
        }

        public Task AnnounceScores() =>
            SendMulticastMessage(new HatPointsUpdated {GuidToPoints = GenerateGuidToPoints()});

        public void SetTimerForExplanation()
        {
            _currentExplanationId = Guid.NewGuid();
            _timer.RequestEventIn(_timeToExplain, new HatTimerFinish(),
                _currentExplanationId.Value); // TODO Correct Guid management
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
            if (IsManged)
                await Manger!.HandleServerMessage(message);
        }

        public Task TellToUnderstander(HatServerMessage message) => Understander.HandleServerMessage(message);

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

        public int IncrementLapCountIfNeeded() =>
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
                x => x.Client.Id,
                x => x.Score
            );
    }
}