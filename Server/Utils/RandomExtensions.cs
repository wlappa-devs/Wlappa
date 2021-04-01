using System;

namespace Server.Utils
{
    public static class RandomExtensions
    {
        public static long NextLong(this Random rng) => rng.Next() << 32 | rng.Next();
    }
}