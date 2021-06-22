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
        private ISubscriptionManager<InGameClientMessage> _subscriptionManager;
        private Guid _hostId;
        private List<MockChannelToClient> _clients;
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

            _subscriptionManager = new MockSubscriptionManager<InGameClientMessage>();

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

            var hostClient = new MockChannelToClient("Shrek");
            
            _hostId = hostClient.Id; 
            
            _lobby = _lobbyFactory.Create(
                new HatGameFactory(_hatLogger),
                _hatConfig,
                _hostId, 
                (_) =>
                {
                    _hasFinished = true;
                }
            );

            _clients = new List<MockChannelToClient> {hostClient, new ("Kek"), new ("Lol")};
        }

        [Test]
        public void TestConnectPlayer()
        {
            _lobby.ConnectPlayer(_clients[0]);
            var joinedLobbyMessage = (JoinedLobby) _clients[0].Messages.Last(message => message is JoinedLobby);
            Assert.AreEqual(_hatConfig.Type, joinedLobbyMessage.Type);
            Assert.True(joinedLobbyMessage.IsHost); // Добавить случай с хост/не хост
            Assert.AreEqual(new List<String>{"player", "manager", "observer", "kukold"}, joinedLobbyMessage.AvailableRoles);
        }

        [Test]
        public void TestStartGame()
        {
            _lobby.ConnectPlayer(_clients[0]);
            _lobby.ConnectPlayer(_clients[1]);
            
            foreach (var client in _clients.Take(2)) _lobby.EventHandle(client.Id, new ReadyChecked()).Wait();
            
            var lobbyUpdateMessage = (LobbyUpdate) _clients[0].Messages.Last(message => message is LobbyUpdate);
            Assert.AreEqual(_clients.Count - 1, lobbyUpdateMessage.Players.Count);
            
            _lobby.EventHandle(_hostId, new StartGame()).Wait();
            
            _lobby.ConnectPlayer(_clients[2]);
            var gameAlreadyStartedMessage = (GameAlreadyStarted)_clients[2].Messages.Last(message => message is GameAlreadyStarted);
            Assert.NotNull(gameAlreadyStartedMessage);
            
            // Тут должна быть проверка на то, что игра уж создана, но до моменты создания игры будто не доходит (await)
            
        }

        [Test]
        public void TestChangeRole()
        {
            _lobby.ConnectPlayer(_clients[0]);
            _lobby.EventHandle(_hostId, new ChangeRole{}).Wait();
        }// роли нет в списке ролей, роль из 
        
        [Test]
        public void TestDisconnect()
        {
            _lobby.ConnectPlayer(_clients[0]);
            _lobby.EventHandle(_hostId, new Disconnect()).Wait();
        }
        
        [Test]
        public void TestReadyChecked()
        {
            _lobby.ConnectPlayer(_clients[0]);
            _lobby.EventHandle(_hostId, new ReadyChecked()).Wait();
        }
        
        
        
        // Проверка с ролями?
        // Проверка HandleGameFinish
    }
}