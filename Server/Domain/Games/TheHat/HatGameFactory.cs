using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Server.Domain.Games.Meta;
using Shared.Protos;
using Shared.Protos.HatSharedClasses;

namespace Server.Domain.Games.TheHat
{
    public class HatGameFactory : IGameFactory
    {
        private readonly ILogger<HatGame> _logger;

        public HatGameFactory(ILogger<HatGame> logger)
        {
            _logger = logger;
        }

        public string DefaultRole => HatRolePlayer.Value;

        public Game Create(GameConfiguration config, GameCreationPayload payload, Func<Task> finished)
        {
            switch (config)
            {
                case HatConfiguration hatConfiguration:
                    var timer = new Timer();
                    var instance = new HatGame(hatConfiguration, payload, finished, timer, new Random(), _logger);
                    timer.Game = instance;
                    return instance;
                default:
                    throw new ArgumentOutOfRangeException(nameof(config));
            }
        }

        public IReadOnlyList<string> Roles =>
            new[]
            {
                HatRolePlayer.Value,
                HatRoleManager.Value,
                // HatRoleObserver.Value,
                HatRoleSpectator.Value
            };

        public GameTypes Type => GameTypes.TheHat;

        public string? ValidateConfig(GameConfiguration config, IValidationPayload payload) =>
            config switch
            {
                HatConfiguration hatConfiguration => ValidateConfig(hatConfiguration, payload),
                _ => "Game configuration from another game"
            };

        private static string? ValidateConfig(HatConfiguration config, IValidationPayload payload)
        {
            if (config.TimeToExplain.Seconds < 1)
                return "Time to explain should be at least 1 second. U r not FLASH";
            if (config.WordsToBeWritten < 1)
                return "Words to be written should be at least 1. U r not dumb, I hope";
            if (config.HatGameModeConfiguration is HatPairChoosingModeConfiguration
                && payload.PlayerToRole.Values.Count(x => x == HatRolePlayer.Value) % 2 == 0)
                return "Not even number of players";
            if (payload.PlayerToRole.Values.Count(x => x == HatRoleManager.Value) > 1)
                return "Too many managers in the game";
            if (payload.PlayerToRole.Values.Count(x => x == HatRolePlayer.Value) < 2)
                return "Not enough players to play!";
            return null;
        }
    }
}