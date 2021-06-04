using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Server.Routing;
using Server.Routing.Helpers;
using Shared.Protos;

namespace Server.Games.Meta
{
    public abstract class Game
    {
        protected abstract Task UnsafeHandleEvent(IInGameClient? client, InGameClientMessage e);

        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        protected virtual Task UnsafeInitialize() => Task.CompletedTask;

        private async Task PerformLockOperation(Func<Task> action)
        {
            await _semaphore.WaitAsync();
            try
            {
                await action();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public Task Initialize() => PerformLockOperation(UnsafeInitialize);

        public Task HandleEvent(IInGameClient? client, InGameClientMessage e) =>
            PerformLockOperation(() => UnsafeHandleEvent(client, e));
    }

    public class GameCreationPayload
    {
        public GameCreationPayload(IReadOnlyDictionary<Guid, string> playerToRole)
        {
            PlayerToRole = playerToRole;
        }

        public IReadOnlyDictionary<Guid, string> PlayerToRole { get; }
    }

    public interface IGameFactory
    {
        public string DefaultRole { get; }

        Game Create(GameConfiguration config, GameCreationPayload payload, IReadOnlyCollection<IInGameClient> clients,
            Func<Task> finished);

        IReadOnlyList<string> Roles { get; }
        GameTypes Type { get; }

        string? ValidateConfig(GameConfiguration config, GameCreationPayload payload);
    }

    public interface IGameEvent
    {
    }

    public interface IPlayerEvent : IGameEvent
    {
        IPlayer Player { get; }
    }
}