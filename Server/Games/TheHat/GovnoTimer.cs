using System;
using System.Diagnostics;
using System.Threading;
using Google.Protobuf.WellKnownTypes;
using Server.Games.TheHat.GameCore;
using Server.Games.TheHat.GameCore.Timer;
using Shared.Protos;
using Shared.Protos.HatSharedClasses;

namespace Server.Games.TheHat
{
    public class GovnoTimer : ITimer
    {
        public void RequestEventIn(Duration duration, InGameClientMessage command)
        {
            throw new NotImplementedException();
        }
    }
}