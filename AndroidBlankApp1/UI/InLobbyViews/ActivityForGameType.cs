using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Shared.Protos;

namespace AndroidBlankApp1
{
    public static class ActivityForGameType
    {
        public static IReadOnlyDictionary<GameTypes, Type> GameTypeToActivity = new Dictionary<GameTypes, Type>()
        {
            {GameTypes.TheHat, typeof(HatWordsChooserActivity)}
        };
    }
}