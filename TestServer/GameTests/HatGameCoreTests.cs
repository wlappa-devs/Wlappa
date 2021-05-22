using System;
using System.Linq;
using Google.Protobuf.WellKnownTypes;
using NUnit.Framework;
using Server.Games.TheHat.GameCore;

namespace TestServer.GameTests
{
    public class HatGameCoreTests
    {
        private HatGame _gameInstance;
        private readonly HatPlayer[] _players = 
        {
            new ("boba",1),
            new ("biba",0),
            new ("lupa",3),
            new ("pupa",2),
        };

        private readonly Word[] _words =
        {
            new ("kek"),
            new ("cheburek"),
            new ("lol"),
            new ("arbidol"),
            new ("1"),
            new ("2"),
            new ("3")
        };

        private readonly TimeSpan _timeToGuess = new (0, 0, 60);

        private MockTimer _timer;

        private bool _hasGameEnded;
        
        [SetUp]
        public void Setup()
        {
            _timer = new MockTimer();
            _gameInstance = new HatGame(_players, _words, PairChoosingMode.Circle, new Random(666), _timer, Duration.FromTimeSpan(_timeToGuess),
                _ => _hasGameEnded = true);
        }

        [Test]
        public void TestCorrectPlayerOrder() => Assert.AreEqual(_gameInstance.Players, _players.OrderBy(player => player.Id));

        [Test]
        public void TestIncorrectCommandFails() => Assert.Throws<ArgumentOutOfRangeException>(() => _gameInstance.TreatCommand(new EndExplanation()));
        
        [Test]
        public void TestCorrectCommandWorks()
        {
            Assert.AreEqual(_hasGameEnded, false);
            _gameInstance.TreatCommand(new EndGame());
            Assert.AreEqual(_hasGameEnded, true);
        }

        [Test]
        public void TestWordsCanBePicked()
        {
            Assert.AreEqual(_gameInstance.CurrentWord, null);
            _gameInstance.TreatCommand(new StartExplanation());
            _gameInstance.TreatCommand(new PickWord());
            Assert.IsNotNull(_gameInstance.CurrentWord);
            Assert.IsEmpty(_gameInstance.GuessedWords);
            _gameInstance.TreatCommand(new GuessRight());
            Assert.IsNotEmpty(_gameInstance.GuessedWords);
            Assert.AreEqual(_words.Length - 1, _gameInstance.WordsRemaining);
        }

        [Test]
        public void TestCorrectGameScenario()
        {
            EmulateGuessingWords(1);
            Assert.AreEqual(_gameInstance.CurrentPair, (1,2));
            Assert.IsNull(_gameInstance.CurrentWord);
            EmulateGuessingWords(1);
            Assert.AreEqual(_gameInstance.CurrentPair, (2,3));
            EmulateGuessingWords(2);
            EmulateGuessingWords(1);
            Assert.AreEqual(_gameInstance.CurrentPair, (0,2));
            while (_gameInstance.WordsRemaining > 1)
            {
                EmulateGuessingWords(1);
            }
            _gameInstance.TreatCommand(new StartExplanation());
            _gameInstance.TreatCommand(new PickWord());
            _gameInstance.TreatCommand(new GuessRight());
            Assert.AreEqual(_hasGameEnded, true);
        }

        private void EmulateGuessingWords(int numOfWords)
        {
            _gameInstance.TreatCommand(new StartExplanation());
            _gameInstance.TreatCommand(new PickWord());
            for (var _ = 0; _ < numOfWords; _++)
            {
                _gameInstance.TreatCommand(new GuessRight());
                _gameInstance.TreatCommand(new PickWord());
            }
            _gameInstance.TreatCommand(_timer.CallBack());
            _gameInstance.TreatCommand(new MoveToNextPlayerPair());
        }
    }
}