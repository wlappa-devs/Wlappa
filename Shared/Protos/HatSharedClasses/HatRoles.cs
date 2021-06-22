using System;

namespace Shared.Protos.HatSharedClasses
{
    public interface IHatRole
    {
        public string StringValue { get; }

        public static IHatRole GetRoleByString(string role) => role switch
        {
            "player" => new HatRolePlayer(),
            "manager" => new HatRoleManager(),
            "observer" => new HatRoleObserver(),
            "spectator" => new HatRoleSpectator(),
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
        // ReSharper disable once MemberCanBePrivate.Global
        public static string Value => "observer";
        public string StringValue => Value;
    }

    public struct HatRoleSpectator : IHatRole
    {
        public static string Value => "spectator";
        public string StringValue => Value;
    }
}