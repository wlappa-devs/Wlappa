using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Shared.Protos;

namespace Server.Domain.Games.Meta
{
    public abstract class Game
    {
        protected abstract Task UnsafeHandleEvent(IInGameClientInteractor? client, InGameClientMessage e);

        private readonly SemaphoreSlim _semaphore = new(1, 1);

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

        public Task HandleEvent(IInGameClientInteractor? client, InGameClientMessage e) =>
            PerformLockOperation(() => UnsafeHandleEvent(client, e));
    }

    public interface IValidationPayload
    {
        public IReadOnlyDictionary<Guid, string> PlayerToRole { get; }
    }

    public class GameCreationPayload : IValidationPayload
    {
        public GameCreationPayload(IReadOnlyDictionary<Guid, string> playerToRole,
            IReadOnlyCollection<IInGameClientInteractor> clients)
        {
            PlayerToRole = playerToRole;
            Clients = clients;
        }

        public IReadOnlyDictionary<Guid, string> PlayerToRole { get; }
        public IReadOnlyCollection<IInGameClientInteractor> Clients { get; }
    }

    public interface IGameFactory
    {
        public string DefaultRole { get; }

        Game Create(GameConfiguration config, GameCreationPayload payload,
            Func<Task> finished);

        IReadOnlyList<string> Roles { get; }
        GameTypes Type { get; }

        string? ValidateConfig(GameConfiguration config, IValidationPayload payload);
    }
}