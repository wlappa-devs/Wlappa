using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.Support.Design.Widget;
using Android.Util;
using Android.Views;
using AndroidBlankApp1.ViewModels.Providers;
using Client_lib;
using Shared.Protos;

namespace AndroidBlankApp1.ViewModels
{
    public class LobbyViewModel
    {
        // TODO handle lobby exit
        private readonly GameViewModelFactory _gameProvider;
        public Action LobbyUpdate { private get; set; }
        public Action GameStarted { private get; set; }
        public Action<string>? MakeSnackBar { private get; set; }

        private readonly Lobby? _lobby;
        private bool _processingLobbyEvents;
        public GameTypes LobbyGameType => _lobby!.Type;
        public IReadOnlyList<string> Roles => _lobby!.AvailableRoles;
        public bool? AmHost => _lobby?.AmHost;
        public IReadOnlyCollection<PlayerInLobby> LastLobbyStatus => _lobby?.LastLobbyStatus;
        public GameConfiguration Configuration { get; set; }
        public Guid LobbyId => _lobby!.LobbyId;

        public LobbyViewModel(LobbyProvider lobbyProvider, GameViewModelFactory gameProvider)
        {
            _gameProvider = gameProvider;
            _lobby = lobbyProvider.Lobby;
            Configuration = lobbyProvider.Configuration;
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
            _lobby.ConfigurationInvalid += msg => { MakeSnackBar?.Invoke("Configuration invalid: " + msg); };
            _lobby.GameFinished += () => _gameProvider.InvalidateGameInstance();
            await _lobby.StartProcessing();
        }

        public async Task HandlePlayerRoleChange(Guid guid, string s)
        {
            if (!_lobby!.AmHost) return;
            await _lobby!.ChangeRole(guid, s);
        }

        public async Task HandleGameStartButtonPressing(View view)
        {
            await _lobby!.StartGame();
        }
    }
}