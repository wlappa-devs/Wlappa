using System;
using System.Threading.Channels;
using System.Threading.Tasks;
using Shared.Protos;

namespace Client_lib
{
    public class Game
    {
        private readonly ChannelWriter<ClientMessage> _requestWriter;
        public Guid PlayerId { get; }
        public event Action<InGameServerMessage>? MessageFromServer;

        public Game(ChannelWriter<ClientMessage> requestWriter, Guid playerId)
        {
            _requestWriter = requestWriter;
            PlayerId = playerId;
        }

        public void HandleGameEvent(InGameServerMessage evt)
        {
            MessageFromServer?.Invoke(evt);
        }

        public async Task SendGameEvent(InGameClientMessage message)
        {
            await _requestWriter.WriteAsync(message);
        }
    }
}