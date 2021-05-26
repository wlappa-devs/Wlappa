using System;

namespace Client_lib
{
    public abstract class ClientLibException : Exception
    {
    }

    public class GameAlreadyStartedException : ClientLibException
    {
    }

    public class LobbyNotFoundException : ClientLibException
    {
    }
}