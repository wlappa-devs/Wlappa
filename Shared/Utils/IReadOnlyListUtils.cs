using System.Collections.Generic;

namespace Shared.Utils
{
    public static class ReadOnlyListUtils
    {
        public static int IndexOf<T>(this IEnumerable<T> self, T elementToFind)
        {
            var i = 0;
            foreach (var element in self)
            {
                if (Equals(element, elementToFind))
                    return i;
                i++;
            }

            return -1;
        }
    }
}