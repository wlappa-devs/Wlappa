using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Server.Application.ChainOfResponsibilityUtils;
using Server.Domain.Games.Clicker;
using Server.Domain.Games.TheHat;
using Server.Domain.Lobby;
using Shared.Protos;
using Shared.Protos.HatSharedClasses;

namespace TestServer.GameTests
{
    public class LobbyTests
    {
#pragma warning disable 8618
        private Lobby _lobby;
        private LobbyFactory _lobbyFactory;
        private GameConfiguration _hatConfig;
        private GameConfiguration _clickerConfig;
        private bool _hasFinished;
        private MockSubscriptionManager<InGameClientMessage> _subscriptionManager;
        private Guid _hostId;
        private List<MockChannelToClient> _clients;
        private ILogger<Lobby> _logger;
        private ILoggerFactory _loggerFactory;
#pragma warning restore 8618
        
        [SetUp]
        public void Setup()
        {
            var serviceProvider = new ServiceCollection()
                .AddLogging()
                .BuildServiceProvider();

            _loggerFactory = serviceProvider.GetService<ILoggerFactory>()!;
            _logger = _loggerFactory.CreateLogger<Lobby>();
            var hatLogger = _loggerFactory.CreateLogger<HatGame>();

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
                ClicksToWin = 2
            };

            var hostClient = new MockChannelToClient("Shrek");
            
            _hostId = hostClient.Id; 
            
            _lobby = _lobbyFactory.Create(
                new HatGameFactory(hatLogger),
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
            ConnectZeroAndFirstPlayers();
            
            var joinedLobbyMessageClient0 = (JoinedLobby) _clients[0].Messages.Last(message => message is JoinedLobby);
            
            Assert.AreEqual(_hatConfig.Type, joinedLobbyMessageClient0.Type);
            
            var joinedLobbyMessageClient1 = (JoinedLobby) _clients[1].Messages.Last(message => message is JoinedLobby);
            
            Assert.True(joinedLobbyMessageClient0.IsHost); 
            Assert.False(joinedLobbyMessageClient1.IsHost); 
            Assert.AreEqual(new List<String>{"player", "manager", "observer", "kukold"}, joinedLobbyMessageClient0.AvailableRoles);
        }

        [Test]
        public void TestStartGame()
        {
            ConnectZeroAndFirstPlayers();
            
            foreach (var client in _clients.Take(2)) _lobby.EventHandle(client.Id, new ReadyChecked()).Wait();
            
            var lobbyUpdateMessage = (LobbyUpdate) _clients[0].Messages.Last(message => message is LobbyUpdate);
            Assert.AreEqual(_clients.Count - 1, lobbyUpdateMessage.Players.Count);
            
            _lobby.EventHandle(_hostId, new StartGame()).Wait();
            
            _lobby.ConnectPlayer(_clients[2]);
            
            var gameAlreadyStartedMessage = (GameAlreadyStarted)_clients[2].Messages.Last(message => message is GameAlreadyStarted);
            Assert.NotNull(gameAlreadyStartedMessage);
        }

        [Test]
        public void TestChangeRoleToCorrect()
        {
            ConnectZeroAndFirstPlayers();
            _lobby.EventHandle(_hostId, new ChangeRole{NewRole = "manager", PlayerId = _clients[0].Id}).Wait();
            
            var lobbyUpdateMessage = (LobbyUpdate) _clients[0].Messages.Last(message => message is LobbyUpdate);
            
            Assert.AreEqual(lobbyUpdateMessage.Players.First(player => player.Id == _hostId).Role, "manager");
            Assert.AreEqual(lobbyUpdateMessage.Players.First(player => player.Id != _hostId).Role, "player");
        }
        
        [Test]
        public void TestChangeRoleToIncorrect()
        {
            _lobby.ConnectPlayer(_clients[0]);
            
            _lobby.EventHandle(_hostId, new ChangeRole{NewRole = "ti hto", PlayerId = _clients[0].Id}).Wait();
            
            var lobbyUpdateMessage = (LobbyUpdate) _clients[0].Messages.Last(message => message is LobbyUpdate);
            Assert.True(lobbyUpdateMessage.Players.FirstOrDefault()?.Role != "ti hto");
        }
        
        [Test]
        public void TestDisconnect()
        {
            ConnectZeroAndFirstPlayers();
            
            foreach (var client in _clients.Take(2)) _lobby.EventHandle(client.Id, new ReadyChecked()).Wait();

            _lobby.EventHandle(_clients[1].Id, new Disconnect()).Wait();
            
            var lobbyUpdateMessageDisconnect = (LobbyUpdate) _clients[0].Messages.Last(message => message is LobbyUpdate);
            Assert.AreEqual(1, lobbyUpdateMessageDisconnect.Players.Count);
            
            _lobby.EventHandle(_clients[0].Id, new Disconnect()).Wait();
            Assert.True(_hasFinished);
        }
        
        [Test]
        public void TestReadyChecked()
        {
            ConnectZeroAndFirstPlayers();
            
            _lobby.EventHandle(_clients[1].Id, new ReadyChecked()).Wait();
            
            var lobbyUpdateMessage = (LobbyUpdate) _clients[0].Messages.Last(message => message is LobbyUpdate);
            Assert.False(lobbyUpdateMessage.Players.First(player => player.Id == _clients[0].Id).IsReady);
            Assert.True(lobbyUpdateMessage.Players.First(player => player.Id == _clients[1].Id).IsReady);
        }

        [Test]
        public void TestGameFinish()
        {
            
            
            var clickerLogger = _loggerFactory.CreateLogger<ClickGame>();
            var clickerLobby = _lobbyFactory.Create(
                new ClickGameFactory(clickerLogger),
                _clickerConfig,
                _hostId, 
                _ => {}
            );
            
            clickerLobby.ConnectPlayer(_clients[0]);
            clickerLobby.ConnectPlayer(_clients[1]);
            
            foreach (var client in _clients.Take(2)) clickerLobby.EventHandle(client.Id, new ReadyChecked()).Wait();
            clickerLobby.EventHandle(_hostId, new StartGame()).Wait();
            
            var lobbyUpdateMessage = (LobbyUpdate) _clients[0].Messages.Last(message => message is LobbyUpdate);

            foreach (var player in lobbyUpdateMessage.Players) Assert.False(player.IsReady);

            _subscriptionManager.Subscriber.EventHandle(_clients[0].Id, new ClickerTimePassedEvent()).Wait();
            _subscriptionManager.Subscriber.EventHandle(_clients[0].Id, new ClickerTimePassedEvent()).Wait();
            _subscriptionManager.Subscriber.EventHandle(_clients[0].Id, new ClickerTimePassedEvent()).Wait();

            var gameZeroClientFinishedMessage = (GameFinished) _clients[0].Messages.Last(message => message is GameFinished);
            var gameFirstClientFinishedMessage = (GameFinished) _clients[1].Messages.Last(message => message is GameFinished);
            

            Assert.NotNull(gameZeroClientFinishedMessage);
            Assert.NotNull(gameFirstClientFinishedMessage);
        }

        private void ConnectZeroAndFirstPlayers()
        {
            _lobby.ConnectPlayer(_clients[0]);
            _lobby.ConnectPlayer(_clients[1]);
        }
    }
}