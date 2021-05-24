using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using NUnit.Framework;
using Server.Games.Meta;
using Server.Games.TheHat;
using Shared.Protos;
using Shared.Protos.HatSharedClasses;
namespace TestServer.GameTests
{
    public class HatIGameTests
    {
        private IGame _gameInstance;
        private HatIGame _hiddenHatGame;
        private HatConfiguration _configuration;
        private List<MockInGameClient> _clients;
        private bool _hasFinished;
        private MockTimer _timer;
        private MockInGameClient _timerEventSender;
        private ICollection<string> _words;
        
        [SetUp]
        public void Setup()
        {
            _hasFinished = false;
            _configuration = new HatConfiguration
            {
                TimeToExplain = Duration.FromTimeSpan(new TimeSpan(0, 0, 30)),
                HatGameModeConfiguration = new HatCircleChoosingModeConfiguration(),
                WordsToBeWritten = 2
            };
            _clients = new List<MockInGameClient>
            {
                new ("Shrek"),
                new ("Shrek2"),
                new ("ShrekTheThird")
            };
            _words = new[] {"kek", "cheburek", "lol", "arbidol", "dirokol", "honker"};
            _timer = new MockTimer();
            _timerEventSender = new MockInGameClient("__TIMER_777___");
            _hiddenHatGame = new HatIGame(
                _configuration, 
                new GameCreationPayload(new Dictionary<Guid, string>()),
                _clients,
                () =>
                {
                    _hasFinished = true;
                    return Task.CompletedTask;
                }, 
                _timer,
                new Random(666),
                null //TODO proper logger
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
            Assert.ThrowsAsync<Exception>(async () => await _gameInstance.HandleEvent(_clients[0], new InGameClientMessage()));
            Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await _gameInstance.HandleEvent(_clients[0], new HatClientMessage()));
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
                var pairMessage = (AnnounceNextPair)client.Messages.Last(message => message is AnnounceNextPair);
                Assert.AreEqual(pairMessage.Explainer, _clients[0].Id);
                Assert.AreEqual(pairMessage.Understander, _clients[1].Id);
            }
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
            
            GetReadyAndGuessWords(1);
            Assert.AreEqual(_hiddenHatGame.WordsRemaining, _words.Count-1);
            GetReadyAndGuessWords(2);
            Assert.AreEqual(_hiddenHatGame.WordsRemaining, _words.Count-3);
            GetReadyAndGuessWords(1);
            Assert.AreEqual(_hiddenHatGame.CurrentPair, (0,2));
            Assert.AreEqual(_hiddenHatGame.LapCount, 2);
            
            Assert.IsNull(_clients[0].Messages.LastOrDefault(msg => msg is FinishMessage));

            GetReadyAndGuessWords(2);
            Assert.AreEqual(_hiddenHatGame.WordsRemaining,0);

            Assert.IsNotNull(_clients[0].Messages.LastOrDefault(msg => msg is FinishMessage));
            Assert.IsTrue(_hasFinished);
        }

        private (MockInGameClient explainer, MockInGameClient understander) GetReady()
        {
            var currentPair = GetCurrentPair();
            var currentExplainer = _clients.First(client => client.Id == currentPair.Explainer);
            var currentUnderstander = _clients.First(client => client.Id == currentPair.Understander);
            _gameInstance.HandleEvent(currentExplainer, new ClientIsReady()).Wait();
            _gameInstance.HandleEvent(currentUnderstander, new ClientIsReady()).Wait();
            return (currentExplainer, currentUnderstander);
        }

        private AnnounceNextPair GetCurrentPair() => 
            (AnnounceNextPair)_clients[0].Messages.Last(message => message is AnnounceNextPair);

        private void GetReadyAndGuessWords(int correctlyGuessed)
        {
            var (explainer, understander) = GetReady();
            for (var _ = 0; _ < correctlyGuessed; _++)
            {
                _gameInstance.HandleEvent(explainer, new GuessRight()).Wait();
            }
            _gameInstance.HandleEvent(explainer, new TimerFinish()).Wait(); //TODO CURRENTLY WE DON'T HAVE TIMER EVENT SENDER
        }
        
        private void FillGameWithWords()
        {
            var wordsPool = new Queue<string>(_words);
            foreach (var client in _clients)
            {
                _gameInstance.HandleEvent(client,
                    new AddWords {Value = new[] {wordsPool.Dequeue(), wordsPool.Dequeue()}}).Wait();
            }
        }
    }
}