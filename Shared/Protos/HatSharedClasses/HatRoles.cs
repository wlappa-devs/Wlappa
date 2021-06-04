using System;

namespace Shared.Protos.HatSharedClasses
{
    public interface IHatRole
    {
        public string StringValue { get; }
        public static string Value => "player";

        public static IHatRole GetRoleByString(string role) => role switch
        {
            "player" => new HatRolePlayer(),
            "manager" => new HatRoleManager(),
            "observer" => new HatRoleObserver(),
            "kukold" => new HatRoleKukold(),
            _ => throw new ArgumentOutOfRangeException(role, "Unexpected role")
        };
    }
    
    public struct HatRolePlayer : IHatRole
    {
        public static string Value => "player";
        public string StringValue => Value;
    }

    public struct HatRoleManager : IHatRole
    {
        public static string Value => "manager";
        public string StringValue => Value;
    }

    public struct HatRoleObserver : IHatRole
    {
        public static string Value => "observer";
        public string StringValue => Value;
    }

    public struct HatRoleKukold : IHatRole
    {
        public static string Value => "kukold";
        public string StringValue => Value;
    }
}