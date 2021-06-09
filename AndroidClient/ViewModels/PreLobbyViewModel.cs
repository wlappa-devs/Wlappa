using System;
using System.Threading.Tasks;
using AndroidClient.ViewModels.Providers;
using Client_lib;
using Shared.Protos;

namespace AndroidClient.ViewModels
{
    public class PreLobbyViewModel
    {
        public Action? LobbyCreated { private get; set; }
        public Action? ShouldSelectLobby { private get; set; }
        public Action? JoinedLobby { private get; set; }
        public Action? ShouldConfigureGame { private get; set; }
        private bool _isConnected;
        public string? Name { get; set; }
        private Guid? _guid;

        public string? Id
        {
            set => _guid = value is null ? (Guid?) null : Guid.Parse(value);
        }

        public GameConfiguration? Configuration { get; set; }
        public event Action<string>? ShowNotification;

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

        public async Task JoinLobby()
        {
            if (!_guid.HasValue) //Name should be validated earlier
            {
                ShowNotification?.Invoke("Invalid Id");
                return;
            }

            await ConnectToServer();
            await _client.ChangeName(Name!);
            _provider.Lobby = await _client.JoinGame(_guid.Value);
            _provider.Configuration = Configuration;
            JoinedLobby?.Invoke();
        }

        public async Task CreateLobby()
        {
            if (Configuration is null)
            {
                ShowNotification?.Invoke("Invalid configuration");
                return;
            }

            await ConnectToServer();
            _guid = await _client.CreateGame(Configuration);
            await JoinLobby();
        }

        public void HandleCreateLobbyButton()
        {
            if (Name is null || Name == "")
            {
                ShowNotification?.Invoke("Enter your name");
                return;
            }

            LobbyCreated?.Invoke();
        }

        public void HandleJoinLobbyButton()
        {
            if (Name is null || Name == "")
            {
                ShowNotification?.Invoke("Enter your name");
                return;
            }

            ShouldSelectLobby?.Invoke();
        }

        public void HandleGameSelected()
        {
            ShouldConfigureGame?.Invoke();
        }
    }
}