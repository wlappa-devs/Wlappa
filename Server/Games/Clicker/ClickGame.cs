using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Server.Games.Meta;
using Server.Routing;
using Server.Routing.Helpers;
using Shared.Protos;

namespace Server.Games.Clicker
{
    public class ClickGame : IGame
    {
        private readonly ClickGameConfiguration _config;
        private readonly Func<Task> _finished;
        private readonly ILogger<ClickGame> _logger;
        private long _currentValue;
        private readonly MulticastGroup _allPlayers;

        public ClickGame(ClickGameConfiguration config, GameCreationPayload payload,
            IReadOnlyCollection<IInGameClient> players, Func<Task> finished, ILogger<ClickGame> logger)
        {
            _config = config;
            _finished = finished;
            _logger = logger;
            _allPlayers = new MulticastGroup(players);
        }

        public async Task HandleEvent(IInGameClient client, InGameClientMessage e)
        {
            _logger.LogInformation("Got increment");
            switch (e)
            {
                case IncrementEvent:
                    _currentValue += _config.IncrementValue;
                    _allPlayers.SendMulticastEvent(new NewValueEvent()
                    {
                        Value = _currentValue
                    });
                    if (_currentValue > _config.ClicksToWin) await _finished();
                    break;
                default:
                    throw new ArgumentException("Unsupported event");
            }
        }
    }

    public class ClickGameFactory : IGameFactory
    {
        private readonly ILogger<ClickGame> _logger;

        public ClickGameFactory(ILogger<ClickGame> logger)
        {
            _logger = logger;
        }

        public IGame Create(GameConfiguration config, GameCreationPayload payload, IReadOnlyCollection<IInGameClient> clients,
            Func<Task> finished)
        {
            if (config is not ClickGameConfiguration correctConfig)
                throw new ArgumentException("Provided invalid configuration");
            return new ClickGame(correctConfig, payload, clients, finished, _logger);
        }

        public IReadOnlyList<string> Roles { get; } = new[] {"Clicker"};

        public GameTypes Type => GameTypes.Clicker;

        public string? ValidateConfig(GameConfiguration config, GameCreationPayload payload)
        {
            return null;
        }
    }
}