using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Server.Domain.ChainOfResponsibilityUtils;
using Server.Domain.Games.Meta;
using Shared.Protos;

namespace Server.Domain.Games.Clicker
{
    public class ClickGame : Game
    {
        private readonly ClickGameConfiguration _config;
        private readonly ITimer _timer;
        private readonly Func<Task> _finished;
        private readonly ILogger<ClickGame> _logger;
        private long _currentValue;
        private readonly MulticastGroup _allPlayers;

        public ClickGame(ClickGameConfiguration config, ITimer timer,
            IReadOnlyCollection<IChannelToClient<InGameServerMessage>> players, Func<Task> finished,
            ILogger<ClickGame> logger)
        {
            _config = config;
            _timer = timer;
            _finished = finished;
            _logger = logger;
            _allPlayers = new MulticastGroup(players);
        }

        protected override async Task UnsafeHandleEvent(Guid? _, InGameClientMessage e)
        {
            _logger.LogInformation("Got increment");
            switch (e)
            {
                case ClickerIncrementEvent:
                    _timer.RequestEventIn(TimeSpan.FromSeconds(5), new ClickerTimePassedEvent(), Guid.NewGuid());
                    break;
                case ClickerTimePassedEvent:
                    _currentValue += _config.IncrementValue;
                    await _allPlayers.SendMulticastEvent(new ClickerNewValueEvent
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


        public Game Create(GameConfiguration config, GameCreationPayload payload, Func<Task> finished)
        {
            if (config is not ClickGameConfiguration correctConfig)
                throw new ArgumentException("Provided invalid configuration");
            var timer = new Timer();
            var instance = new ClickGame(correctConfig, timer, payload.Clients, finished, _logger);
            timer.Game = instance;
            return instance;
        }

        public IReadOnlyList<string> Roles { get; } = new[] {"Clicker"};
        public string DefaultRole => Roles[0];

        public GameTypes Type => GameTypes.Clicker;

        public string? ValidateConfig(GameConfiguration config, IValidationPayload payload)
        {
            return null;
        }
    }
}