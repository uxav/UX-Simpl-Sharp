using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace UXLib.Displays
{
    public class ElectricScreenRelaysMomentary : ElectricScreen
    {
        public ElectricScreenRelaysMomentary(Crestron.SimplSharpPro.Relay upRelay, Crestron.SimplSharpPro.Relay downRelay)
            : base (upRelay, downRelay, UXLib.Relays.UpDownRelayModeType.Momentary)
        {
            
        }
    }
}