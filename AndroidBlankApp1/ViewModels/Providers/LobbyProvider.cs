using Client_lib;
using Shared.Protos;

namespace AndroidBlankApp1.ViewModels.Providers
{
    public class LobbyProvider
    {
        public Lobby? Lobby { get; set; }
        public GameConfiguration? Configuration { get; set; }
    }
}