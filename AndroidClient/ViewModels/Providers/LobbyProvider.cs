using Client_lib;
using Shared.Protos;

namespace AndroidClient.ViewModels.Providers
{
    public class LobbyProvider
    {
        public Lobby? Lobby { get; set; }
        public GameConfiguration? Configuration { get; set; }
    }
}