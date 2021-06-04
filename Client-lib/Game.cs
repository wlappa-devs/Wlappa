using System;
using System.Collections.Concurrent;
using System.Threading.Channels;
using System.Threading.Tasks;
using Shared.Protos;

namespace Client_lib
{
    public class Game
    {
        private readonly ChannelWriter<ClientMessage> _requestWriter;
        private Action<InGameServerMessage>? _messageFromServer;
        private ConcurrentQueue<InGameServerMessage> _messageQueue = new ConcurrentQueue<InGameServerMessage>();
        public Guid PlayerId { get; }

        public Action<InGameServerMessage>? MessageFromServer
        {
            get => _messageFromServer;
            set
            {
                _messageFromServer = value;
                if (value is null) return;
                while (!_messageQueue.IsEmpty)
                    if (_messageQueue.TryDequeue(out var message))
                        value(message);
            }
        }

        public Game(ChannelWriter<ClientMessage> requestWriter, Guid playerId)
        {
            _requestWriter = requestWriter;
            PlayerId = playerId;
        }

        public void HandleGameEvent(InGameServerMessage evt) //TODO Something wrong with naming
        {
            if (MessageFromServer is null)
            {
                _messageQueue.Enqueue(evt);
            }

            MessageFromServer?.Invoke(evt);
        }

        public async Task SendGameEvent(InGameClientMessage message)
        {
            await _requestWriter.WriteAsync(message);
        }
    }
}