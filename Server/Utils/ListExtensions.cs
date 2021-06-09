using System.Collections.Generic;

namespace Server.Utils
{
    public static class ListExtensions
    {
        public static T RemoveAtAndReturn<T>(this List<T> list, int index)
        {
            var lastValue = list[index];
            list.RemoveAt(index);
            return lastValue;
        }
    }
}