﻿using System;
using System.Collections.Generic;
using AndroidClient.UI.GamesViews.Hat.WordsChooser;
using Shared.Protos;

namespace AndroidClient.UI.InLobbyViews
{
    public static class ActivityForGameType
    {
        public static readonly IReadOnlyDictionary<GameTypes, Type> GameTypeToActivity = new Dictionary<GameTypes, Type>()
        {
            {GameTypes.TheHat, typeof(HatWordsChooserActivity)}
        };
    }
}