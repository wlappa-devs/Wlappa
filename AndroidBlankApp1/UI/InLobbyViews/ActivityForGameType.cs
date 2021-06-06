using System;
using System.Collections.Generic;
using AndroidBlankApp1.UI.GamesViews.Hat;
using Shared.Protos;

namespace AndroidBlankApp1.UI.InLobbyViews
{
    public static class ActivityForGameType
    {
        public static IReadOnlyDictionary<GameTypes, Type> GameTypeToActivity = new Dictionary<GameTypes, Type>()
        {
            {GameTypes.TheHat, typeof(HatWordsChooserActivity)}
        };
    }
}