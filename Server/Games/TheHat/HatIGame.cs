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
        // TODO Add explanation cancellation
        // TODO Check hat words availability at the timer event
        private readonly Func<Task> _finished;
        private readonly ITimer _timer;
        private readonly Random _random;
        private readonly ILogger<HatIGame> _logger;
        private readonly int _wordsToBeWritten;
        private readonly HatGameModeConfiguration _mode;
        private readonly TimeSpan _timeToExplain;

        private IHatGameState _currentState;
        private readonly Dictionary<IInGameClient, HatPlayer> _clientToPlayerMapping;
        private readonly List<Word> _words;

        public bool IsManged => Manger is not null;
        public HatMember? Manger;
        private readonly List<HatPlayer> _players = new();
        private readonly List<HatMember> _observers = new();
        private readonly List<HatMember> _kukolds = new();

        private MulticastGroup _membersWhoNeedsToKnowTheWordExceptExplainer;
        private readonly MulticastGroup _allMembers;

        public HatPlayer Understander => _players[CurrentPair.understanderIndex];
        public HatPlayer Explainer => _players[CurrentPair.explainerIndex];
        public Word? CurrentWord { get; private set; }
        public int WordsRemaining => _words.Count;
        public (int explainerIndex, int understanderIndex) CurrentPair { get; set; }
        public int PlayersCount => _players.Count;
        public int LapCount { get; private set; } = 1;

        public HatIGame(HatConfiguration configuration, GameCreationPayload payload,
            IReadOnlyCollection<IInGameClient> players, Func<Task> finished, ITimer timer, Random random,
            ILogger<HatIGame> logger)
        {
            _finished = finished;
            _timer = timer;
            _random = random;
            _logger = logger;
            _wordsToBeWritten = configuration.WordsToBeWritten;
            _mode = configuration.HatGameModeConfiguration;
            _timeToExplain = configuration.TimeToExplain;

            #region PayLoad Deconstruction

            players.Select((player, index) =>
            {
                var playerRole = HatRole.GetRoleByString(payload.PlayerToRole[player.Id]);
                if (playerRole is HatRolePlayer)
                    _players.Add(new HatPlayer(player, index, playerRole));
                else if (playerRole is HatRoleObserver)
                    _observers.Add(new HatMember(player, playerRole));
                else if (playerRole is HatRoleKukold)
                    _kukolds.Add(new HatMember(player, playerRole));
                else if (playerRole is HatRoleManager)
                    Manger = new HatMember(player, playerRole);

                return 1;
            }).ToList();

            #endregion

            _allMembers = new MulticastGroup(players);
            _membersWhoNeedsToKnowTheWordExceptExplainer = new MulticastGroup(_kukolds.Select(x => x.Client).ToList());

            _words = new List<Word>();
            _clientToPlayerMapping = _players
                .ToDictionary(player => player.Client, player => player);
            _currentState = new AddingWordsState(0, this);
        }

        public override async Task HandleEvent(IInGameClient? client, InGameClientMessage e)
        {
            if (e is HatClientMessage hatClientMessage)
                if (client is null)
#pragma warning disable 8625
                    _currentState = await _currentState.HandleEvent(null, hatClientMessage);
#pragma warning restore 8625

                else
                    _currentState = await _currentState.HandleEvent(_clientToPlayerMapping[client], hatClientMessage);
            else
                throw new Exception("Wrong game, dude");
        }

        public bool ValidateWords(IReadOnlyList<string> words) => words.Count == _wordsToBeWritten;

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
            _allMembers.SendMulticastEvent(message);

        public Task AnnounceCurrentPair() =>
            SendMulticastMessage(new HatAnnounceNextPair
            {
                Explainer = Explainer.Client.Id,
                Understander = Understander.Client.Id
            });

        public Task AnnounceScores() =>
            SendMulticastMessage(new HatPointsUpdated {GuidToPoints = GenerateGuidToPoints()});

        public void SetTimerForExplanation()
        {
            _timer.RequestEventIn(_timeToExplain, new HatTimerFinish(), Guid.NewGuid()); // TODO Correct Guid management
            SendMulticastMessage(new HatExplanationStarted());
        }

        public Word? TakeWord()
        {
            if (WordsRemaining == 0)
            {
                SendMulticastMessage(new HatNoWordsLeft());
                _finished();
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

        public void MoveToNextPair()
        {
            if (_mode.GameIsOver(LapCount, CurrentPair.explainerIndex, PlayersCount))
            {
                SendMulticastMessage(new HatRotationFinished());
                return;
            }

            if (_mode is HatCircleChoosingModeConfiguration)
                CurrentPair = GetNextPairCircle();
            if (_mode is HatPairChoosingModeConfiguration)
                CurrentPair = GetNextPairPairs();
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


        private int IncrementLapCount() => ++LapCount;

        public int IncrementLapCountIfNeeded() =>
            CurrentPair.explainerIndex == PlayersCount - 1
                ? IncrementLapCount()
                : -1;

        public void ReturnCurrentWordInHatIfNeeded()
        {
            if (CurrentWord is not null)
            {
                _words.Add(CurrentWord);
                CurrentWord = null;
            }
        }

        public Dictionary<Guid, int> GenerateGuidToPoints() =>
            _players.ToDictionary(
                x => x.Client.Id,
                x => x.Score
            );
    }
}