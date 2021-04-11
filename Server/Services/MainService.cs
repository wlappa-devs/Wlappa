using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Grpc.Core;
using GrpcService;
using Microsoft.Extensions.Logging;
using Server.Games.Clicker;
using Server.Games.Meta;
using Server.Routing;
using Server.Routing.Helpers;
using Server.Utils;
using MainController = GrpcService.MainController;
using ServerMessage = GrpcService.ServerMessage;

namespace Server.Services
{
    public class MainService : MainController.MainControllerBase
    {
        private readonly ILogger<MainService> _logger;
        private readonly Random _rng;
        private readonly Routing.MainController _mainController;

        private static IReadOnlyDictionary<GameType, IGrpcAdaptingStrategy> _typeToStrategy =
            new Dictionary<GameType, IGrpcAdaptingStrategy>()
            {
                {GameType.Clicker, new ClickerGrpcAdaptingStrategy()},
            };

        private static IReadOnlyDictionary<Configuration.GameConfigurationOneofCase, GameType> _confToType =
            new Dictionary<Configuration.GameConfigurationOneofCase, GameType>()
            {
                {Configuration.GameConfigurationOneofCase.ClickGameConfiguration, GameType.Clicker}
            };

        private static IReadOnlyDictionary<GameEvent.EventOneofCase, GameType> _eventToType =
            new Dictionary<GameEvent.EventOneofCase, GameType>()
            {
                {GameEvent.EventOneofCase.ClickGameEvent, GameType.Clicker}
            };

        public MainService(ILogger<MainService> logger, Random rng, Routing.MainController mainController)
        {
            _logger = logger;
            _rng = rng;
            _mainController = mainController;
        }

        public IGrpcAdaptingStrategy SelectStrategy(ClientMessage message)
        {
            var type = message.PayloadCase switch
            {
                ClientMessage.PayloadOneofCase.Configuration => _confToType
                    [message.Configuration.GameConfigurationCase],
                ClientMessage.PayloadOneofCase.GameEvent => _eventToType[message.GameEvent.EventCase],
                ClientMessage.PayloadOneofCase.Connect => _mainController.GetGameTypeForCode(
                    Guid.Parse(message.Connect.GameCode)),
                _ => throw new ArgumentOutOfRangeException(nameof(message))
            };
            return _typeToStrategy[type];
        }

        public override async Task Connect(IAsyncStreamReader<ClientMessage> requestStream,
            IServerStreamWriter<ServerMessage> responseStream, ServerCallContext context)
        {
            var initialMessage = requestStream.Current;
            await requestStream.MoveNext();
            var connectionMessage = requestStream.Current;
            await requestStream.MoveNext();
            if (
                !(connectionMessage.PayloadCase == ClientMessage.PayloadOneofCase.Configuration ||
                  connectionMessage.PayloadCase == ClientMessage.PayloadOneofCase.Connect) ||
                initialMessage.PayloadCase != ClientMessage.PayloadOneofCase.Greeting
            )
            {
                // TODO reject connection
                return;
            }

            var strategy = SelectStrategy(connectionMessage);
            IInitialMessage parsedConnectionMessage;
            switch (connectionMessage.PayloadCase)
            {
                case ClientMessage.PayloadOneofCase.Configuration:
                    parsedConnectionMessage =
                        strategy.ParseConfigurationMessage(connectionMessage.Configuration);
                    break;
                case ClientMessage.PayloadOneofCase.Connect:
                    parsedConnectionMessage = GrpcEventConverterHelper.ConvertInitialMessage(connectionMessage.Connect);
                    break;
                default:
                    throw new IndexOutOfRangeException();
            }

            var player = new GrpcPlayerAdapter(_rng.NextLong(), initialMessage.Greeting.PlayerName, responseStream);
            var connection = new GrpcPlayerConnectionAdapter(requestStream, player, strategy);
            _mainController.Connect(player, connection, parsedConnectionMessage, strategy);
            connection.Start().Start();
        }
    }
}