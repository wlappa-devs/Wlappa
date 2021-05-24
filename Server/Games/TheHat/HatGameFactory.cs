using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Server.Games.Meta;
using Server.Routing.Helpers;
using Shared.Protos;
using Shared.Protos.HatSharedClasses;

namespace Server.Games.TheHat
{
    public class HatGameFactory : IGameFactory
    {
        private readonly ILogger<HatIGame> _logger;

        public HatGameFactory(ILogger<HatIGame> logger)
        {
            _logger = logger;
        }

        public string DefaultRole => HatRolePlayer.Value;

        public IGame Create(GameConfiguration config, GameCreationPayload payload,
            IReadOnlyCollection<IInGameClient> clients,
            Func<Task> finished) => config switch
        {
            HatConfiguration hatConfiguration =>
                new HatIGame(hatConfiguration, payload, clients, finished,
                    new GovnoTimer(), new Random(), _logger),
            _ => throw new ArgumentOutOfRangeException(nameof(config))
        };

        public IReadOnlyList<string> Roles =>
            new[] {HatRolePlayer.Value, HatRoleManager.Value, HatRoleObserver.Value, HatRoleKukold.Value};

        public GameTypes Type => GameTypes.TheHat;

        public string? ValidateConfig(GameConfiguration config, GameCreationPayload payload) =>
            config switch
            {
                HatConfiguration hatConfiguration => ValidateConfig(hatConfiguration, payload),
                _ => "Game configuration from another game"
            };

        public string? ValidateConfig(HatConfiguration config, GameCreationPayload payload)
        {
            if (config.TimeToExplain.Seconds < 1)
                return "Time to explain should be at least 1 second. U r not FLASH";
            if (config.WordsToBeWritten < 1)
                return "Words to be written should be at least 1. U r not dumb, I hope";
            if (config.HatGameModeConfiguration is HatPairChoosingModeConfiguration
                && payload.PlayerToRole.Values.Count(x => x == "player") % 2 == 0)
                return "Not even number of players";
            if (payload.PlayerToRole.Values.Count(x => x == HatRoleManager.Value) > 1)
                return "Too many managers in the game";
            return null;
        }
    }
}