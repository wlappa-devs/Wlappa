using System;
using GrpcService;
using Server.Games.Clicker;
using Server.Games.Meta;

namespace Server.Routing.Helpers
{
    public static class GrpcEventConverterHelper
    {
        public static IPlayerEvent ConvertEvent(ClickGameEvent e, IPlayer player)
        {
            return e.EventCase switch
            {
                ClickGameEvent.EventOneofCase.IncrementEvent => new Games.Clicker.IncrementEvent(player),
                _ => throw new ArgumentOutOfRangeException(nameof(e), "Unsupported event")
            };
        }

        public static IInitialMessage ConvertInitialMessage(Configuration message, IGrpcAdaptingStrategy strategy) =>
            strategy.ParseConfigurationMessage(message);

        public static IInitialMessage ConvertInitialMessage(Connect message) => new GameJoin(message.GameCode);
    }
}