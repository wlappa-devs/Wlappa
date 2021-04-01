using System;
using System.Threading.Tasks;
using Grpc.Core;
using GrpcService;
using Server.Games.Meta;

namespace Server.Routing.Helpers
{
    public class GrpcPlayerAdapter : IPlayer
    {
        private IServerStreamWriter<ServerMessage> _streamWriter;

        public GrpcPlayerAdapter(long id, string name, IServerStreamWriter<ServerMessage> streamWriter)
        {
            Id = id;
            Name = name;
            _streamWriter = streamWriter;
        }

        public long Id { get; }
        public string Name { get; }

        public void HandleEvent(IToPlayerEvent e)
        {
            throw new NotImplementedException();
        }
    }

    public class GrpcPlayerConnectionAdapter : IPlayerConnection
    {
        private readonly IAsyncStreamReader<ClientMessage> _requestStream;
        private readonly GrpcPlayerAdapter _player;
        private readonly IGrpcAdaptingStrategy _strategy;
        private event Action<IPlayerEvent>? Notify;

        public GrpcPlayerConnectionAdapter(
            IAsyncStreamReader<ClientMessage> requestStream,
            GrpcPlayerAdapter player,
            IGrpcAdaptingStrategy strategy)
        {
            _player = player;
            _requestStream = requestStream;
            _strategy = strategy;
        }

        public async Task Start()
        {
            await foreach (var clientMessage in _requestStream.ReadAllAsync())
            {
                if (clientMessage.PayloadCase != ClientMessage.PayloadOneofCase.GameEvent)
                    throw new ArgumentOutOfRangeException();
                Notify?.Invoke(_strategy.ParsePlayerEvent(clientMessage.GameEvent, _player));
            }
        }

        public void AddEventListener(Action<IPlayerEvent> listener)
        {
            Notify += listener;
        }
    }
}