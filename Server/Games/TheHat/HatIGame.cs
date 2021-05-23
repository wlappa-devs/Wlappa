using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;
using Server.Games.Meta;
using Server.Games.TheHat.GameCore;
using Server.Games.TheHat.GameCore.Timer;
using Server.Games.TheHat.HatIGameStates;
using Server.Routing;
using Server.Routing.Helpers;
using Server.Utils;
using Shared.Protos;
using Shared.Protos.HatSharedClasses;

namespace Server.Games.TheHat
{
    public class HatIGame : IGame
    {
        private readonly Func<Task> _finished;
        private readonly ITimer _timer;
        private readonly Random _random;
        private readonly ILogger<HatIGame> _logger;
        private readonly int _wordsToBeWritten;
        private readonly PairChoosingMode _mode;
        private readonly Duration _timeToExplain;

        private IHatGameState _currentState;
        private readonly Dictionary<Client, HatPlayer> _clientToPlayerMapping;
        private readonly MulticastGroup _allPlayers;
        private readonly List<HatPlayer> _players;
        private readonly List<Word> _words;

        public Word? CurrentWord { get; private set; }
        public int WordsRemaining => _words.Count;
        public (int explainerIndex, int understanderIndex) CurrentPair { get; set; }
        public int PlayersCount => _players.Count;

        public HatIGame(HatConfiguration configuration, GameCreationPayload payload,
            IReadOnlyCollection<Client> players, Func<Task> finished, ITimer timer, Random random,
            ILogger<HatIGame> logger)
        {
            _finished = finished;
            _timer = timer;
            _random = random;
            _logger = logger;
            _wordsToBeWritten = configuration.WordsToBeWritten;
            _mode = configuration.PairChoosingMode;
            _timeToExplain = configuration.TimeToExplain;

            _allPlayers = new MulticastGroup(players);
            _players = players.Select((x, i) => new HatPlayer(x, i)).ToList();
            _currentState = new AddingWordsState(0, this);
        }

        public async Task HandleEvent(Client client, InGameClientMessage e)
        {
            if (e is HatClientMessage hatClientMessage)
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
            else
            {
                await author.HandleServerMessage(new WrongWordsAmount());
                return 0;
            }
        }

        public Task SendMulticastMessage(HatServerMessage message) =>
            _allPlayers.SendMulticastEvent(message);

        public Task AnnounceCurrentPair() =>
            _allPlayers.SendMulticastEvent(new AnnounceNextPair
            {
                Explainer = _players[CurrentPair.explainerIndex].Client.Id,
                Understander = _players[CurrentPair.understanderIndex].Client.Id
            });

        public void SetTimerForExplanation()
        {
            _timer.RequestEventIn(_timeToExplain, new TimerFinish());
            _allPlayers.SendMulticastEvent(new ExplanationStarted());
        }

        public Word? TakeWord()
        {
            if (WordsRemaining == 0)
                return CurrentWord;

            var randomIndex = _random.Next(0, _words.Count);
            (_words[^1], _words[randomIndex]) = (_words[randomIndex], _words[^1]);
            _words[^1].AddGuessTry();
            return CurrentWord = _words.RemoveAtAndReturn(_words.Count - 1);
        }

        public Task TellToExplainer(HatServerMessage message) =>
            _players[CurrentPair.explainerIndex].HandleServerMessage(message);

        public Task TellToUnderstander(HatServerMessage message) =>
            _players[CurrentPair.understanderIndex].HandleServerMessage(message);
    }

    public class HatGameFactory : IGameFactory
    {
        private readonly ILogger<HatIGame> _logger;

        public HatGameFactory(ILogger<HatIGame> logger)
        {
            _logger = logger;
        }

        public IGame Create(GameConfiguration config, GameCreationPayload payload, IReadOnlyCollection<Client> clients,
            Func<Task> finished) => config switch
        {
            HatConfiguration hatConfiguration =>
                new HatIGame(hatConfiguration, payload, clients, finished,
                    new GovnoTimer(), new Random(), _logger),
            _ => throw new ArgumentOutOfRangeException(nameof(config))
        };

        public IReadOnlyList<string> Roles => new[] {"player"}; //TODO "observer", "manager"
        public GameTypes Type => GameTypes.TheHat;

        public string? ValidateConfig(GameConfiguration config, GameCreationPayload payload) =>
            config switch
            {
                HatConfiguration hatConfiguration => ValidateConfig(hatConfiguration, payload),
                _ => "Game configuration from another game"
            };

        public string? ValidateConfig(HatConfiguration config, GameCreationPayload payload)
        {
            if (config.TimeToExplain.Seconds < 1)
                return "Time to explain should be at least 1 second. U r not FLASH";
            if (config.WordsToBeWritten < 1)
                return "Words to be written should be at least 1. U r not dumb, I hope";
            return null;
        }
    }
}