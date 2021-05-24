using System;
using System.Reactive;

namespace Server.Games.TheHat
{
    public interface HatRole
    {
        public static string Value { get; }

        public static HatRole GetRoleByString(string role) => role switch
        {
            "player" => new HatRolePlayer(),
            "manager" => new HatRoleManager(),
            "observer" => new HatRoleObserver(),
            "kukold" => new HatRoleKukold(),
            _ => throw new ArgumentOutOfRangeException(role, "Unexpected role")
        };
    }
    
    public struct HatRolePlayer : HatRole
    {
        public static string Value => "player";
    }

    public struct HatRoleManager : HatRole
    {
        public static string Value => "manager";
    }

    public struct HatRoleObserver : HatRole
    {
        public static string Value => "observer";
    }

    public struct HatRoleKukold : HatRole
    {
        public static string Value => "kukold";
    }
}