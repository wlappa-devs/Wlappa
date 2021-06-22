using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using NUnit.Framework.Internal;
using Server.Domain.Games.Meta;
using Server.Domain.Games.TheHat;
using Shared.Protos;
using Shared.Protos.HatSharedClasses;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace TestServer.GameTests
{
    public class HatIGameTests
    {
        private Game _gameInstance;
        private HatGame _hiddenHatGame;
        private HatConfiguration _configuration;
        private List<MockChannelToClient> _clients;
        private bool _hasFinished;
        private HatGameFactory _factory;
        private MockTimer _timer;
        private ICollection<string> _words;
        private ILogger<HatGame> _logger;

        [SetUp]
        public void Setup()
        {
            var serviceProvider = new ServiceCollection()
                .AddLogging()
                .BuildServiceProvider();

            var factory = serviceProvider.GetService<ILoggerFactory>();

            _logger = factory.CreateLogger<HatGame>();

            _factory = new HatGameFactory(_logger);
            _hasFinished = false;
            _configuration = new HatConfiguration
            {
                TimeToExplain = new TimeSpan(0, 0, 30),
                HatGameModeConfiguration = new HatCircleChoosingModeConfiguration(),
                WordsToBeWritten = 2
            };
            _clients = new List<MockChannelToClient>
            {
                new("Shrek"),
                new("Shrek2"),
                new("ShrekTheThird")
            };
            _words = new[] {"kek", "cheburek", "lol", "arbidol", "dirokol", "honker"};
            _timer = new MockTimer();
            _hiddenHatGame = new HatGame(
                _configuration,
                new GameCreationPayload(_clients.ToDictionary(x => x.Id, x => _factory.DefaultRole), _clients),
                () =>
                {
                    _hasFinished = true;
                    return Task.CompletedTask;
                },
                _timer,
                new Random(666),
                _logger
            );
            _gameInstance = _hiddenHatGame;
        }

        [Test]
        public void TestCorrectPlayerCount()
        {
            Assert.True(_hiddenHatGame.PlayersCount == _clients.Count);
        }

        [Test]
        public void TestIncorrectCommandFails()
        {
            Assert.ThrowsAsync<ArgumentException>(async () =>
                await _gameInstance.EventHandle(_clients[0].Id, new InGameClientMessage()));
            Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
                await _gameInstance.EventHandle(_clients[0].Id, new HatClientMessage()));
        }

        [Test]
        public void TestFillGameWithWords()
        {
            Assert.DoesNotThrow(FillGameWithWords);
        }

        [Test]
        public void TestCorrectGameStartingEvents()
        {
            FillGameWithWords();
            foreach (var client in _clients)
            {
                Assert.IsNotEmpty(client.Messages);
                var pairMessage = (HatAnnounceNextPair) client.Messages.Last(message => message is HatAnnounceNextPair);
                Assert.AreEqual(pairMessage.Explainer, _clients[0].Id);
                Assert.AreEqual(pairMessage.Understander, _clients[1].Id);
            }
        }

        [Test]
        public void TestWordCanceled()
        {
            FillGameWithWords();
            GetReadyAndGuessWordsWithCancel(1);
            Assert.AreEqual(_hiddenHatGame.WordsRemaining, _words.Count - 1);
            GetReadyAndGuessWordsWithCancel(0);
            Assert.AreEqual(_hiddenHatGame.WordsRemaining, _words.Count - 1);
        }

        [Test]
        public void TestSimpleGameScenario()
        {
            FillGameWithWords();
            Assert.IsNotEmpty(_clients[0].Messages);
            var pairMessage = GetCurrentPair();
            Assert.AreEqual(pairMessage.Explainer, _clients[0].Id);
            Assert.AreEqual(pairMessage.Understander, _clients[1].Id);
            Assert.AreEqual(_hiddenHatGame.WordsRemaining, _words.Count);
            Assert.AreEqual(_hiddenHatGame.LapCount, 1);

            GetReadyAndGuessWordsWithTimer(1);
            Assert.AreEqual(_hiddenHatGame.WordsRemaining, _words.Count - 1);
            GetReadyAndGuessWordsWithTimer(2);
            Assert.AreEqual(_hiddenHatGame.WordsRemaining, _words.Count - 3);
            GetReadyAndGuessWordsWithTimer(1);
            Assert.AreEqual(_hiddenHatGame.CurrentPair, (0, 2));
            Assert.AreEqual(_hiddenHatGame.LapCount, 2);

            Assert.IsNull(_clients[0].Messages.LastOrDefault(msg => msg is HatFinishMessage));

            GetReadyAndGuessWordsWithTimer(2);
            Assert.AreEqual(_hiddenHatGame.WordsRemaining, 0);

            Assert.IsNotNull(_clients[0].Messages.LastOrDefault(msg => msg is HatFinishMessage));
            Assert.IsTrue(_hasFinished);
        }

        private (MockChannelToClient explainer, MockChannelToClient understander) GetReady()
        {
            var currentPair = GetCurrentPair();
            var currentExplainer = _clients.First(client => client.Id == currentPair.Explainer);
            var currentUnderstander = _clients.First(client => client.Id == currentPair.Understander);
            _gameInstance.EventHandle(currentExplainer.Id, new HatClientIsReady()).Wait();
            _gameInstance.EventHandle(currentUnderstander.Id, new HatClientIsReady()).Wait();
            return (currentExplainer, currentUnderstander);
        }

        private HatAnnounceNextPair GetCurrentPair() =>
            (HatAnnounceNextPair) _clients[0].Messages.Last(message => message is HatAnnounceNextPair);

        private void GetReadyAndGuessWordsWithTimer(int correctlyGuessed)
        {
            var (explainer, understander) = GetReady();
            for (var _ = 0; _ < correctlyGuessed; _++)
            {
                _gameInstance.EventHandle(explainer.Id, new HatGuessRight()).Wait();
            }

            _gameInstance.EventHandle(null, new HatTimerFinish())
                .Wait();
        }
        
        private void GetReadyAndGuessWordsWithCancel(int correctlyGuessed)
        {
            var (explainer, understander) = GetReady();
            for (var _ = 0; _ < correctlyGuessed; _++)
            {
                _gameInstance.EventHandle(explainer.Id, new HatGuessRight()).Wait();
            }

            _gameInstance.EventHandle(explainer.Id, new HatCancelExplanation()).Wait();
        }
        
        private void FillGameWithWords()
        {
            var wordsPool = new Queue<string>(_words);
            foreach (var client in _clients)
            {
                _gameInstance.EventHandle(client.Id,
                    new HatAddWords {Value = new[] {wordsPool.Dequeue(), wordsPool.Dequeue()}}).Wait();
            }
        }
    }
}