using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Server.Domain.Games.Meta;
using Shared.Protos;

namespace Server.Application
{
    public class GameResolver
    {
        private readonly ImmutableArray<IGameFactory> _games;

        public GameResolver(IEnumerable<IGameFactory> games)
        {
            _games = games.ToImmutableArray();
        }

        public IGameFactory FindGameFactory(GameTypes type) => _games.First(g => g.Type == type);
    }
}