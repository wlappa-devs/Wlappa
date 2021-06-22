﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AndroidClient.ViewModels.Providers;
using Client_lib;
using Shared.Protos;

namespace AndroidClient.ViewModels
{
    public class LobbyViewModel
    {
        private readonly LobbyProvider _lobbyProvider;
        private readonly GameViewModelFactory _gameProvider;
        public Action? LobbyUpdate { private get; set; }
        public Action? GameStarted { private get; set; }
        public Action<string>? MakeSnackBar { private get; set; }

        public Action<string>? LobbyDestroyed { private get; set; }

        // Left in case we need to change configuration from lobby
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        // ReSharper disable once MemberCanBePrivate.Global
        public GameConfiguration? Configuration { get; set; }

        private Lobby? _lobby;
        private bool _processingLobbyEvents;
        public GameTypes LobbyGameType => _lobby!.Type;
        public IReadOnlyList<string> Roles => _lobby!.AvailableRoles;
        public bool? AmHost => _lobby?.AmHost;
        public IReadOnlyCollection<PlayerInLobby>? LastLobbyStatus => _lobby?.LastLobbyStatus;
        public Guid LobbyId => _lobby!.LobbyId;

        public LobbyViewModel(LobbyProvider lobbyProvider, GameViewModelFactory gameProvider)
        {
            _lobbyProvider = lobbyProvider;
            _gameProvider = gameProvider;
        }

        public void InitializeLobby()
        {
            _lobby = _lobbyProvider.Lobby;
            Configuration = _lobbyProvider.Configuration;
        }

        public async void StartProcessingEvents()
        {
            if (_processingLobbyEvents) return;
            if (_lobby is null) return;
            _processingLobbyEvents = true;
            _lobby.HandleGameStart += game =>
            {
                _gameProvider.GameInstance = game;
                _gameProvider.Players = _lobby.LastLobbyStatus;
                GameStarted?.Invoke();
            };
            _lobby.LobbyUpdate += () => { LobbyUpdate?.Invoke(); };
            _lobby.GameStartingProblems += msg => { MakeSnackBar?.Invoke("Game Starting problems: " + msg); };
            _lobby.GameFinished += () => _gameProvider.InvalidateGameInstance();
            _lobby.LobbyDestroyed += async msg =>
            {
                await DisconnectFromLobby();
                LobbyDestroyed?.Invoke(msg);
            };

            try
            {
                await _lobby.StartProcessing();
            }
            catch (OperationCanceledException)
            {
            }
        }

        public async Task HandlePlayerRoleChange(Guid guid, string s)
        {
            if (!_lobby!.AmHost) return;
            await _lobby!.ChangeRole(guid, s);
        }

        public async Task HandleGameStartButtonPressing()
        {
            await _lobby!.StartGame();
        }

        public async Task HandlePlayerReadinessChange(bool oneWayReady)
        {
            await _lobby!.ReadyChecked(oneWayReady);
        }

        public async Task DisconnectFromLobby()
        {
            if (_lobby is null) return;
            await _lobby.Disconnect();
            _lobby = null;
            Configuration = null;
            LobbyUpdate = null;
            MakeSnackBar = null;
            GameStarted = null;
            _processingLobbyEvents = false;
        }

        public void InvalidateGameInstance()
        {
            _gameProvider.InvalidateGameInstance();
        }
    }
}