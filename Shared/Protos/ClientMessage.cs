using System;
using System.Runtime.Serialization;
using Shared.Protos.HatSharedClasses;
using ProtoBuf;

namespace Shared.Protos
{
    [DataContract]
    [ProtoContract]
    [ProtoInclude(1, typeof(PreGameClientMessage))]
    [ProtoInclude(2, typeof(LobbyClientMessage))]
    [ProtoInclude(3, typeof(InGameClientMessage))]
    public class ClientMessage
    {
    }

    [DataContract]
    [ProtoContract]
    [ProtoInclude(1, typeof(Greeting))]
    [ProtoInclude(2, typeof(CreateLobby))]
    [ProtoInclude(3, typeof(JoinGame))]
    public abstract class PreGameClientMessage : ClientMessage
    {
    }

    [DataContract]
    [ProtoContract]
    [ProtoInclude(1, typeof(ChangeRole))]
    [ProtoInclude(2, typeof(StartGame))]
    [ProtoInclude(3, typeof(Disconnect))]
    public abstract class LobbyClientMessage : ClientMessage
    {
    }

    [DataContract]
    [ProtoContract]
    public class Greeting : PreGameClientMessage
    {
        [ProtoMember(1)] public string Name { get; set; }
    }

    [DataContract]
    [ProtoContract]
    public enum GameTypes
    {
        [EnumMember]
        [ProtoEnum]
        Clicker = 0,
        [EnumMember]
        [ProtoEnum]
        TheHat = 1
    }

    [DataContract]
    [ProtoContract]
    [ProtoInclude(1, typeof(ClickGameConfiguration))]
    [ProtoInclude(2, typeof(HatConfiguration))]
    public abstract class GameConfiguration
    {
    }

    [DataContract]
    [ProtoContract]
    public class CreateLobby : PreGameClientMessage
    {
        [ProtoMember(1)] public GameTypes Type { get; set; }
        [ProtoMember(2)] public GameConfiguration Configuration { get; set; }
    }

    [DataContract]
    [ProtoContract]
    public class JoinGame : PreGameClientMessage
    {
        [ProtoMember(1)] public Guid Id { get; set; }
    }

    [DataContract]
    [ProtoContract]
    public class ChangeRole : LobbyClientMessage
    {
        [ProtoMember(1)] public Guid PlayerId { get; set; }
        [ProtoMember(2)] public string NewRole { get; set; }
    }

    [DataContract]
    [ProtoContract]
    public class StartGame : LobbyClientMessage
    {
    }

    [DataContract]
    [ProtoContract]
    public class Disconnect : LobbyClientMessage
    {
    }

    [DataContract]
    [ProtoContract]
    [ProtoInclude(1, typeof(ClickerClientMessage))]
    [ProtoInclude(2, typeof(HatClientMessage))]
    public class InGameClientMessage : ClientMessage
    {
    }
}