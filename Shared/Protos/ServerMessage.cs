using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using ProtoBuf;
using Shared.Protos.HatSharedClasses;

namespace Shared.Protos
{
    [DataContract]
    [ProtoInclude(1, typeof(PreGameServerMessage))]
    [ProtoInclude(2, typeof(LobbyServerMessage))]
    [ProtoInclude(3, typeof(InGameServerMessage))]
    public abstract class ServerMessage
    {
    }

    [DataContract]
    [ProtoInclude(1, typeof(GreetingSuccessful))]
    [ProtoInclude(2, typeof(LobbyCreated))]
    public abstract class PreGameServerMessage : ServerMessage
    {
    }

    [DataContract]
    [ProtoInclude(1, typeof(GameAlreadyStarted))]
    [ProtoInclude(2, typeof(JoinedLobby))]
    [ProtoInclude(3, typeof(LobbyNotFound))]
    [ProtoInclude(4, typeof(LobbyUpdate))]
    [ProtoInclude(5, typeof(ConfigurationInvalid))]
    [ProtoInclude(6, typeof(GameCreated))]
    [ProtoInclude(7, typeof(GameFinished))]
    public abstract class LobbyServerMessage : ServerMessage
    {
    }

    [DataContract]
    public class GreetingSuccessful : PreGameServerMessage
    {
        [ProtoMember(1)] public Guid Guid { get; init; }
    }

    [DataContract]
    public class LobbyCreated : PreGameServerMessage
    {
        [ProtoMember(1)] public Guid Guid { get; init; }
    }

    [DataContract]
    public class GameAlreadyStarted : LobbyServerMessage
    {
    }

    [DataContract]
    public class JoinedLobby : LobbyServerMessage
    {
        [ProtoMember(1)] public GameTypes Type { get; init; }
        [ProtoMember(2)] public bool IsHost { get; init; }
        [ProtoMember(3)] public IReadOnlyList<string> AvailableRoles { get; init; }
    }

    [DataContract]
    public class LobbyNotFound : LobbyServerMessage
    {
    }

    [DataContract]
    public class PlayerInLobby
    {
        [ProtoMember(1)] public Guid Id { get; init; }
        [ProtoMember(2)] public string Name { get; init; }
        [ProtoMember(3)] public string Role { get; init; }
    }

    [DataContract]
    public class LobbyUpdate : LobbyServerMessage
    {
        [ProtoMember(1)] public IReadOnlyCollection<PlayerInLobby> Players { get; init; }
    }

    [DataContract]
    public class ConfigurationInvalid : LobbyServerMessage
    {
        public string Message { get; init; }
    }

    [DataContract]
    public class GameCreated : LobbyServerMessage
    {
    }

    [DataContract]
    public class GameFinished : LobbyServerMessage
    {
    }

    [DataContract]
    [ProtoInclude(1, typeof(ClickerServerMessage))]
    [ProtoInclude(2, typeof(HatServerMessage))]
    public abstract class InGameServerMessage : ServerMessage
    {
        [ProtoMember(1)] public Dictionary<Guid, int> GuidToPoints { get; init; }
    }
}