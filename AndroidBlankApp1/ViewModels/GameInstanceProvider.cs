using System.Collections.Generic;
using Client_lib;
using Shared.Protos;

namespace AndroidBlankApp1.ViewModels
{
    public class GameInstanceProvider
    {
        public Game? GameInstance { get; set; }
        public IReadOnlyCollection<PlayerInLobby>? Players { get; set; }
    }
}