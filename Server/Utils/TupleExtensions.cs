using System;

namespace Server.Utils
{
    public static class TupleExtensions
    {
        public static (TR explainerIndex, TR understanderIndex) Select<T, TR>(this (T first, T second) tuple, Func<T, TR> selector) =>
            (selector(tuple.first), selector(tuple.second));
    }
}