using System;
using System.Threading.Tasks;
using Android.Support.Design.Widget;
using Android.Views;
using AndroidBlankApp1.ViewModels.Providers;
using Client_lib;
using Shared.Protos;

namespace AndroidBlankApp1.ViewModels
{
    public class PreLobbyViewModel
    {
        public Action LobbyCreated { private get; set; }
        public Action ShouldSelectLobby { private get; set; }
        public Action JoinedLobby { private get; set; }
        public Action ShouldConfigureGame { private get; set; }

        private bool _isConnected = false;


        public string? Name { get; set; }


        private Guid? _guid;

        public string? Id
        {
            get => _guid?.ToString();
            set => _guid = value is null ? (Guid?) null : Guid.Parse(value);
        }

        public GameConfiguration? Configuration { get; set; }

        private readonly Client _client;
        private readonly LobbyProvider _provider;

        public PreLobbyViewModel(Client client, LobbyProvider provider)
        {
            _client = client;
            _provider = provider;
        }

        private async Task ConnectToServer()
        {
            if (_isConnected) return;
            await _client.ConnectToServer("10.0.2.2:5000", Name!);
            _isConnected = true;
        }

        public async Task JoinLobby(View view)
        {
            if (!_guid.HasValue) //Name should be validated earlier
            {
                Snackbar.Make(view, "Invalid Id", 2000).Show();
                return;
            }

            await ConnectToServer();
            await _client.ChangeName(Name);
            _provider.Lobby = await _client.JoinGame(_guid.Value);
            _provider.Configuration = Configuration;
            JoinedLobby?.Invoke();
        }

        public async Task CreateLobby(View view)
        {
            if (Configuration is null)
            {
                Snackbar.Make(view, "Invalid configuration", 2000).Show();
                return;
            }

            await ConnectToServer();
            _guid = await _client.CreateGame(Configuration);
            await JoinLobby(view);
        }

        public void HandleCreateLobbyButton(View view)
        {
            if (Name is null || Name == "")
            {
                Snackbar.Make(view, "Enter your name", 2000).Show();
                return;
            }

            LobbyCreated?.Invoke();
        }

        public void HandleJoinLobbyButton(View view)
        {
            if (Name is null || Name == "")
            {
                Snackbar.Make(view, "Enter your name", 2000).Show();
                return;
            }

            ShouldSelectLobby?.Invoke();
        }

        public void HandleGameSelected(View view)
        {
            ShouldConfigureGame?.Invoke();
        }
    }
}