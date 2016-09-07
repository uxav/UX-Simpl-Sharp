using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using UXLib.Devices.Relays;

namespace UXLib.Devices.Displays
{
    public class ElectricScreenRelaysMomentary : ElectricScreen
    {
        public ElectricScreenRelaysMomentary(Crestron.SimplSharpPro.Relay upRelay, Crestron.SimplSharpPro.Relay downRelay)
            : base(new UpDownRelays(upRelay, downRelay, UXLib.Devices.Relays.UpDownRelayModeType.Momentary)) { }

        public ElectricScreenRelaysMomentary(Crestron.SimplSharpPro.Relay upRelay, Crestron.SimplSharpPro.Relay downRelay, DisplayDevice display)
            : base(new UpDownRelays(upRelay, downRelay, UXLib.Devices.Relays.UpDownRelayModeType.Momentary), display) { }
    }
}