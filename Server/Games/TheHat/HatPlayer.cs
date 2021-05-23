using System.Threading.Tasks;
using Server.Routing.Helpers;
using Shared.Protos.HatSharedClasses;

namespace Server.Games.TheHat.GameCore
{
    public struct HatPlayer
    {
        public int Score { get; private set; }
        public Client Client { get; }
        public int Id { get; }

        public HatPlayer(Client client, int id)
        {
            Client = client;
            Id = id;
            Score = 0;
        }

        public async Task HandleServerMessage(HatServerMessage message)
        {
            await Client.HandleInGameMessage(message);
        }

        public int IncrementScore() => ++Score;
        public int DecrementScore() => Score == 0 ? 0 : --Score;
    }
}