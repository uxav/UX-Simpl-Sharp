using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using UXLib.Relays;

namespace UXLib.Displays
{
    public class ElectricScreen
    {
        protected ElectricScreen(UpDownRelays relays)
        {
            this.relays = relays;
        }

        protected ElectricScreen(Crestron.SimplSharpPro.Relay upRelay, Crestron.SimplSharpPro.Relay downRelay, UpDownRelayModeType modeType)
        {
            this.relays = new UpDownRelays(upRelay, downRelay, modeType);
        }

        UpDownRelays relays;

        public void Up()
        {
            this.relays.Up();
        }

        public void Down()
        {
            this.relays.Down();
        }
    }
}