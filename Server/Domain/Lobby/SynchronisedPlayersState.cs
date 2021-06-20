using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Server.Application.ChainOfResponsibilityUtils;
using Shared.Protos;

namespace Server.Domain.Lobby
{
    internal class SynchronisedPlayersState
    {
        private readonly Dictionary<Guid, string> _playersToRoles = new();
        private Dictionary<Guid, bool> _playersToReadyStatus = new();
        private readonly List<IChannelToClient<LobbyServerMessage>> _players = new();
        private readonly ReaderWriterLockSlim _lock = new();

        public void AddPlayerWithRole(IChannelToClient<LobbyServerMessage> player, string role)
        {
            _lock.EnterWriteLock();
            try
            {
                _players.Add(player);
                _playersToReadyStatus[player.Id] = false;
                _playersToRoles[player.Id] = role;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void ChangePlayerRole(Guid id, string newRole)
        {
            _lock.EnterWriteLock();
            try
            {
                _playersToRoles[id] = newRole;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
        
        public void ChangePlayerReadyStatus(Guid id, bool oneWayReady)
        {
            _lock.EnterWriteLock();
            try
            {
                if (oneWayReady)
                    _playersToReadyStatus[id] = true;
                else
                    _playersToReadyStatus[id] = !_playersToReadyStatus[id];
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void UnreadyEveryone()
        {
            _lock.EnterWriteLock();
            try
            {
                _playersToReadyStatus = _playersToReadyStatus.ToDictionary(kv => kv.Key, _ => false);;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public IReadOnlyList<IChannelToClient<LobbyServerMessage>> RemovePlayer(Guid id)
        {
            _lock.EnterWriteLock();
            try
            {
                _players.RemoveAll(p => p.Id == id);
                _playersToRoles.Remove(id);
                _playersToReadyStatus.Remove(id);
                return _players.ToImmutableArray();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public T ReadWithLock<T>(
            Func<IReadOnlyList<IChannelToClient<LobbyServerMessage>>, IReadOnlyDictionary<Guid, string>, IReadOnlyDictionary<Guid, bool>, T> function)
        {
            _lock.EnterReadLock();
            try
            {
                return function(_players.ToImmutableArray(), _playersToRoles.ToImmutableDictionary(), _playersToReadyStatus.ToImmutableDictionary());
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        ~SynchronisedPlayersState()
        {
            _lock.Dispose();
        }
    }
}