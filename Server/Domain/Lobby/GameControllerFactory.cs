using System;
using Microsoft.Extensions.Logging;
using Server.Domain.Games.Meta;
using Shared.Protos;

namespace Server.Domain.Lobby
{
    public class GameControllerFactory
    {
        private readonly ILogger<GameController> _logger;

        public GameControllerFactory(ILogger<GameController> logger)
        {
            _logger = logger;
        }

        public GameController Create(IGameFactory factory, GameConfiguration config, Guid initialHost, Action finished) =>
            new(factory, config, initialHost, _logger, finished);
    }
}