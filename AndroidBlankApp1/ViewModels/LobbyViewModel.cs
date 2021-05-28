using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.Support.Design.Widget;
using Android.Util;
using Android.Views;
using Client_lib;
using Shared.Protos;

namespace AndroidBlankApp1.ViewModels
{
    public class LobbyViewModel
    {
        private readonly GameInstanceProvider _gameProvider;
        public event Action LobbyUpdate;

        public event Action GameStarted;
        
        private readonly Lobby? _lobby;
        public GameTypes LobbyGameType => _lobby!.Type;
        public IReadOnlyList<string> Roles => _lobby!.AvailableRoles;
        public bool? AmHost => _lobby?.AmHost;
        public IReadOnlyCollection<PlayerInLobby> LastLobbyStatus => _lobby?.LastLobbyStatus;
        public GameConfiguration Configuration { get; set; }
        public Guid LobbyId => _lobby!.LobbyId;

        public LobbyViewModel(LobbyProvider lobbyProvider, GameInstanceProvider gameProvider)
        {
            _gameProvider = gameProvider;
            _lobby = lobbyProvider.Lobby;
            Configuration = lobbyProvider.Configuration;
        }

        public async void StartProcessingEvents(View view)
        {
            
            if (_lobby is null) return;
            _lobby.HandleGameStart += game =>
            {
                _gameProvider.GameInstance = game;
                _gameProvider.Players = _lobby.LastLobbyStatus;
                GameStarted?.Invoke();
            };
            _lobby.LobbyUpdate += () =>
            {
                LobbyUpdate?.Invoke();
            };
            _lobby.ConfigurationInvalid += msg =>
            {
                Snackbar.Make(view, "Configuration invalid: " + msg, 2000).Show();
            };
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