using System;
using System.Threading.Channels;
using System.Threading.Tasks;
using Shared.Protos;

namespace Client_lib
{
    public class Game
    {
        private readonly ChannelWriter<ClientMessage> _requestWriter;
        public event Action<ServerMessage>? MessageFromServer;

        public Game(ChannelWriter<ClientMessage> requestWriter)
        {
            _requestWriter = requestWriter;
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