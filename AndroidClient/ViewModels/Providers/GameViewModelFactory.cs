using System;
using System.Collections.Generic;
using AndroidClient.ViewModels.GameViewModels;
using Client_lib;
using Java.Lang;
using Shared.Protos;

namespace AndroidClient.ViewModels.Providers
{
    public class GameViewModelMappingProvider
    {
        private static readonly Dictionary<Type, Func<Game, IReadOnlyCollection<PlayerInLobby>, IGameViewModel>>
            Mapping =
                new Dictionary<Type, Func<Game, IReadOnlyCollection<PlayerInLobby>, IGameViewModel>>()
                {
                    {typeof(HatViewModel), (game, players) => new HatViewModel(game, players)}
                };

        // ReSharper disable once MemberCanBeMadeStatic.Global
        public Func<Game, IReadOnlyCollection<PlayerInLobby>, IGameViewModel> GetFactoryForType<T>() where T : IGameViewModel =>
            Mapping[typeof(T)];
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
            if (GameInstance is null || Players is null) throw new NullPointerException();
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