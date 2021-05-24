using System;
using System.Runtime.Serialization;
using Shared.Protos.HatSharedClasses;
using ProtoBuf;

namespace Shared.Protos
{
    [DataContract]
    [ProtoInclude(1, typeof(PreGameClientMessage))]
    [ProtoInclude(2, typeof(LobbyClientMessage))]
    [ProtoInclude(3, typeof(InGameClientMessage))]
    public class ClientMessage
    {
    }

    [DataContract]
    [ProtoInclude(1, typeof(Greeting))]
    [ProtoInclude(2, typeof(CreateLobby))]
    [ProtoInclude(3, typeof(JoinGame))]
    public abstract class PreGameClientMessage : ClientMessage
    {
    }

    [DataContract]
    [ProtoInclude(1, typeof(ChangeRole))]
    [ProtoInclude(2, typeof(StartGame))]
    [ProtoInclude(3, typeof(Disconnect))]
    public abstract class LobbyClientMessage : ClientMessage
    {
    }

    [DataContract]
    public class Greeting : PreGameClientMessage
    {
        [ProtoMember(1)] public string Name { get; init; }
    }

    public enum GameTypes
    {
        Clicker = 0,
        TheHat = 1
    }

    [DataContract]
    [ProtoInclude(1, typeof(ClickGameConfiguration))]
    [ProtoInclude(2, typeof(HatConfiguration))]
    public abstract class GameConfiguration
    {
    }

    [DataContract]
    public class CreateLobby : PreGameClientMessage
    {
        [ProtoMember(1)] public GameTypes Type { get; init; }
        [ProtoMember(2)] public GameConfiguration Configuration { get; init; }
    }

    [DataContract]
    public class JoinGame : PreGameClientMessage
    {
        [ProtoMember(1)] public Guid Id { get; init; }
    }

    [DataContract]
    public class ChangeRole : LobbyClientMessage
    {
        [ProtoMember(1)] public Guid PlayerId { get; init; }
        [ProtoMember(2)] public string NewRole { get; init; }
    }

    [DataContract]
    public class StartGame : LobbyClientMessage
    {
    }

    [DataContract]
    public class Disconnect : LobbyClientMessage
    {
    }

    [DataContract]
    [ProtoInclude(1, typeof(ClickerClientMessage))]
    [ProtoInclude(2, typeof(HatClientMessage))]
    public class InGameClientMessage : ClientMessage
    {
    }
}