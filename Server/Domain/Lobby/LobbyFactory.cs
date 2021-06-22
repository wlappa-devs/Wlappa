using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Server.Application;
using Server.Application.ChainOfResponsibilityUtils;
using Server.Domain.Games.Meta;
using Shared.Protos;

namespace Server.Domain.Lobby
{
    public class LobbyFactory
    {
        private readonly ILogger<Lobby> _logger;
        private readonly ISubscriptionManager<InGameClientMessage> _subscriptionManager;

        public LobbyFactory(ILogger<Lobby> logger, ISubscriptionManager<InGameClientMessage> subscriptionManager)
        {
            _logger = logger;
            _subscriptionManager = subscriptionManager;
        }

        public Lobby Create(IGameFactory factory, GameConfiguration config, Guid initialHost,
            Action<IReadOnlyCollection<Guid>>? finished) =>
            new(factory, config, initialHost, _logger, finished, _subscriptionManager);
    }
}