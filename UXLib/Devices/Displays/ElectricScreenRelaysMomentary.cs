using UXLib.Devices.Relays;
using Relay = Crestron.SimplSharpPro.Relay;

namespace UXLib.Devices.Displays
{
    public class ElectricScreenRelaysMomentary : ElectricScreen
    {
        public ElectricScreenRelaysMomentary(Relay upRelay, Relay downRelay)
            : base(new UpDownRelays(upRelay, downRelay, UpDownRelayModeType.Momentary)) { }

        public ElectricScreenRelaysMomentary(Relay upRelay, Relay downRelay, DisplayDevice display)
            : base(new UpDownRelays(upRelay, downRelay, UpDownRelayModeType.Momentary), display) { }
    }
}