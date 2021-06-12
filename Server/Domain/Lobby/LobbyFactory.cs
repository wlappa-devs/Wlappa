using System;
using Microsoft.Extensions.Logging;
using Server.Domain.Games.Meta;
using Shared.Protos;

namespace Server.Domain.Lobby
{
    public class LobbyFactory
    {
        private readonly ILogger<Lobby> _logger;

        public LobbyFactory(ILogger<Lobby> logger)
        {
            _logger = logger;
        }

        public Lobby Create(IGameFactory factory, GameConfiguration config, Guid initialHost, Action finished) =>
            new(factory, config, initialHost, _logger, finished);
    }
}