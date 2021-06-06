using System;
using System.Collections.Generic;
using System.Linq;
using AndroidBlankApp1.ViewModels.GameViewModels;
using Client_lib;
using Shared.Protos;

namespace AndroidBlankApp1.ViewModels.Providers
{
    public class GameViewModelMappingProvider
    {
        private static readonly Dictionary<Type, Func<Game, IReadOnlyCollection<PlayerInLobby>, IGameViewModel>>
            _mapping =
                new Dictionary<Type, Func<Game, IReadOnlyCollection<PlayerInLobby>, IGameViewModel>>()
                {
                    {typeof(HatViewModel), (game, players) => new HatViewModel(game, players)}
                };

        public Func<Game, IReadOnlyCollection<PlayerInLobby>, IGameViewModel> GetFactoryForType<T>() where T : IGameViewModel =>
            _mapping[typeof(T)];
    }

    public class GameViewModelFactory
    {
        private readonly GameViewModelMappingProvider _mappingProvider;
        private IGameViewModel? _viewModelInstance;
        public Game? GameInstance { private get; set; }
        public IReadOnlyCollection<PlayerInLobby>? Players { private get; set; }

        public GameViewModelFactory(GameViewModelMappingProvider mappingProvider)
        {
            _mappingProvider = mappingProvider;
        }

        public T GetViewModel<T>() where T : IGameViewModel
        {
            _viewModelInstance ??= _mappingProvider.GetFactoryForType<T>()(GameInstance, Players);
            return (T) _viewModelInstance;
        }

        public void InvalidateGameInstance()
        {
            GameInstance = null;
            _viewModelInstance = null;
        }
    }
}