using System;
using System.Threading.Tasks;
using Android.Support.Design.Widget;
using Android.Views;
using Client_lib;
using Shared.Protos;

namespace AndroidBlankApp1.ViewModels
{
    public class PreLobbyViewModel
    {
        public event Action LobbyCreateButtonPressed;
        public event Action LobbyCreated;
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

        public async Task JoinServer(View view)
        {
            if (!_guid.HasValue) //Name should be validated earlier
            {
                Snackbar.Make(view, "Invalid Id", 2000).Show();
                return;
            }
            // await _client.ConnectToServer("10.0.2.2:5000", Name!);
            _provider.Lobby = await _client.JoinGame(_guid.Value);
            //TODO notify joining
        }

        public async Task CreateLobby(View view)
        {
            if (Configuration is null)
            {
                Snackbar.Make(view, "Invalid configuration", 2000).Show();
                return;
            }
            _guid = await _client.CreateGame(Configuration);
            await JoinServer(view);
        }

        public void HandleCreateLobbyButton(View view)
        {
            LobbyCreateButtonPressed?.Invoke();
        }
    }
}