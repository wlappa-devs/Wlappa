using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using ProtoBuf;
using Google.Protobuf.WellKnownTypes;
using Shared.Protos.HatSharedClasses;

namespace Shared.Protos
{
    [DataContract]
    [ProtoContract]
    [ProtoInclude(1, typeof(PreGameServerMessage))]
    [ProtoInclude(2, typeof(LobbyServerMessage))]
    [ProtoInclude(3, typeof(InGameServerMessage))]
    public abstract class ServerMessage
    {
    }

    [DataContract]
    [ProtoContract]
    [ProtoInclude(1, typeof(GreetingSuccessful))]
    [ProtoInclude(2, typeof(LobbyCreated))]
    public abstract class PreGameServerMessage : ServerMessage
    {
    }

    [DataContract]
    [ProtoContract]
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
    [ProtoContract]
    public class GreetingSuccessful : PreGameServerMessage
    {
        [ProtoMember(1)] public Guid Guid { get; set; }
    }

    [DataContract]
    [ProtoContract]
    public class LobbyCreated : PreGameServerMessage
    {
        [ProtoMember(1)] public Guid Guid { get; set; }
    }

    [DataContract]
    [ProtoContract]
    public class GameAlreadyStarted : LobbyServerMessage
    {
    }

    [DataContract]
    [ProtoContract]
    public class JoinedLobby : LobbyServerMessage
    {
        [ProtoMember(1)] public GameTypes Type { get; set; }
        [ProtoMember(2)] public bool IsHost { get; set; }
        [ProtoMember(3)] public IReadOnlyList<string> AvailableRoles { get; set; }
    }

    [DataContract]
    [ProtoContract]
    public class LobbyNotFound : LobbyServerMessage
    {
    }

    [DataContract]
    [ProtoContract]
    public class PlayerInLobby
    {
        [ProtoMember(1)] public Guid Id { get; set; }
        [ProtoMember(2)] public string Name { get; set; }
        [ProtoMember(3)] public string Role { get; set; }
    }

    [DataContract]
    [ProtoContract]
    public class LobbyUpdate : LobbyServerMessage
    {
        [ProtoMember(1)] public IReadOnlyCollection<PlayerInLobby> Players { get; set; }
    }

    [DataContract]
    [ProtoContract]
    public class ConfigurationInvalid : LobbyServerMessage
    {
        [ProtoMember(1)] public string Message { get; set; }
    }

    [DataContract]
    [ProtoContract]
    public class GameCreated : LobbyServerMessage
    {
    }

    [DataContract]
    [ProtoContract]
    public class GameFinished : LobbyServerMessage
    {
    }

    [DataContract]
    [ProtoContract]
    [ProtoInclude(1, typeof(ClickerServerMessage))]
    [ProtoInclude(2, typeof(HatServerMessage))]
    public abstract class InGameServerMessage : ServerMessage
    {
    }
}