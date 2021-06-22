using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Server.Application.ChainOfResponsibilityUtils;
using Server.Domain.Games.TheHat;
using Server.Domain.Lobby;
using Shared.Protos;
using Shared.Protos.HatSharedClasses;

namespace TestServer.GameTests
{
    public class LobbyTests
    {
        private Lobby _lobby;

        private LobbyFactory _lobbyFactory;
        private GameConfiguration _hatConfig;
        private GameConfiguration _clickerConfig;
        private bool _hasFinished;
        private SubscriptionManager<InGameClientMessage> _subscriptionManager;
        private Int32 _hostId;
        private ILogger<Lobby> _logger;
        
        [SetUp]
        public void Setup()
        {
            var serviceProvider = new ServiceCollection()
                .AddLogging()
                .BuildServiceProvider();

            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            _logger = loggerFactory.CreateLogger<Lobby>();
            var _hatLogger = loggerFactory.CreateLogger<HatGame>();
            
            // _subscriptionManager = new SubscriptionManager<InGameClientMessage>(resolver: )

            _lobbyFactory = new LobbyFactory(_logger, _subscriptionManager);
            _hasFinished = false;
            
            _hatConfig = new HatConfiguration
            {
                TimeToExplain = new TimeSpan(0, 0, 30),
                HatGameModeConfiguration = new HatCircleChoosingModeConfiguration(),
                WordsToBeWritten = 2
            };

            _clickerConfig = new ClickGameConfiguration
            {
                IncrementValue = 1,
                ClicksToWin = 42
            };

            _lobby = _lobbyFactory.Create(
                new HatGameFactory(_hatLogger),
                _hatConfig,
                Guid.NewGuid(), 
                guids => guids.Select(s => s)
            );
        }

        [Test]
        public void TestCorrectPlayerCount()
        {
            Assert.AreEqual(GameTypes.Clicker, _clickerConfig.Type);
            Assert.AreEqual(GameTypes.TheHat, _hatConfig.Type);
        }
    }
}